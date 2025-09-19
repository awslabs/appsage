using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using System.Text;
using Amazon.BedrockRuntime;
using Amazon.BedrockRuntime.Model;
using System.Text.Json;
using SharpToken;
using System.Threading.RateLimiting;
namespace AppSage.Infrastructure.AI
{
    public class BedrockService : IAIQuery
    {
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(2);


        private static RateLimiter _limiter = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,                 // max tokens available
            TokensPerPeriod = 2,            // tokens replenished every period
            ReplenishmentPeriod = TimeSpan.FromSeconds(2),
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 1000,                // max queued requests waiting
            AutoReplenishment = true          // automatic token refills
        });



        private readonly IAppSageLogger _logger;
        private readonly IAppSageConfiguration _configuration;
        private readonly IAWSCredentialProvider _awsCredentialProvider;

        public BedrockService(IAppSageLogger logger, IAppSageConfiguration configuration, IAWSCredentialProvider credentialProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null. Please provide a valid logger instance.");
            _awsCredentialProvider = credentialProvider ?? throw new ArgumentNullException(nameof(credentialProvider), "Credential provider cannot be null. Please provide a valid credential provider instance.");
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null. Please provide a valid configuration instance.");
            _semaphore = new SemaphoreSlim(_configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:MaxConcurrentThreads"));
        }

        public static class Models
        {
            public const string TitanTextExpress = "amazon.titan-text-express-v1";
            public const string NovaPro = "amazon.nova-pro-v1:0";
            public const string NovaLight = "amazon.nova-lite-v1:0";
            public const string NovaMicro = "amazon.nova-micro-v1:0";
            public const string ClaudeV4 = "anthropic.claude-opus-4-20250514-v1:0";
            public const string JambaLarge = "ai21.jamba-1-5-large-v1:0";
            public const string JambaMini = "ai21.jamba-1-5-mini-v1:0";
        }

        public string Invoke(string prompt)
        {
            //Estimate prompt size uisng gpt -3.5-turbo encoding. This is an approximation good for most models
            var encoding = GptEncoding.GetEncodingForModel("gpt-3.5-turbo");

            // Tokenize and count
            int tokenCount = encoding.Encode(prompt).Count;

            AIQueryConfig queryConfig = new AIQueryConfig();
            //default model;
            queryConfig.ModelId = _configuration.Get<string>("AppSage.Infrastructure.AI.Bedrock:ModelId");
            switch (tokenCount)
            {
                case < 7000:
                    queryConfig.ModelId = Models.TitanTextExpress;
                    break;
                case < 200000:
                    queryConfig.ModelId = Models.NovaLight;
                    break;
                default:
                    queryConfig.ModelId = Models.NovaPro;
                    break;
            }

            queryConfig.MaxTokens = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:MaxTokens");
            queryConfig.Temperature =  _configuration.Get<double>("AppSage.Infrastructure.AI.Bedrock:Temperature");
            queryConfig.TopK = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:TopK");
            queryConfig.TopP = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:TopP");

            return Invoke(prompt, queryConfig);
        }

        public string Invoke(string prompt, AIQueryConfig queryConfig)
        {
            //read default configuration values
            var region = Amazon.RegionEndpoint.GetBySystemName(_configuration.Get<string>("AppSage.Infrastructure.AI.Bedrock:Region"));
            var timeout = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:Timeout");

            try
            {
                // Initialize AWS Bedrock client with credentials and region
                var credentials = _awsCredentialProvider.GetCredentials();
                var config = new AmazonBedrockRuntimeConfig
                {
                    RegionEndpoint = region,
                    Timeout = TimeSpan.FromMilliseconds(timeout)
                };

                var requestBody = GetRequestBody(prompt, queryConfig);
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var contentBytes = Encoding.UTF8.GetBytes(jsonContent);

                var request = new InvokeModelRequest
                {
                    ModelId = queryConfig.ModelId,
                    ContentType = "application/json",
                    Accept = "application/json",
                    Body = new MemoryStream(contentBytes)
                };

                using (var client = new AmazonBedrockRuntimeClient(credentials, config))
                {

                    try
                    {
                        _semaphore.Wait();

                        InvokeModelResponse response = null;
                        using (var lease = _limiter.AcquireAsync(1).GetAwaiter().GetResult())
                        {
                            if (lease.IsAcquired)
                            {
                                _logger.LogInformation("Invoking Bedrock");

                                response = client.InvokeModelAsync(request).Result;
                            }
                            else
                            {
                                _logger.LogError("Rate limit exceeded. Please try again later.");
                            }
                        }

                        using var reader = new StreamReader(response.Body);
                        var responseContent = reader.ReadToEnd();
                        var result = GetResponseOutput(responseContent, queryConfig.ModelId);
                        return result;

                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }


            }
            catch (Exception ex)
            {

                throw new InvalidOperationException($"Failed to invoke Bedrock model: {ex.Message}", ex);
            }

        }

        private static object GetRequestBody(string prompt, AIQueryConfig queryConfig)
        {
            // Create request body based on model type
            object requestBody;
            if (queryConfig.ModelId.StartsWith("amazon.titan"))
            {
                requestBody = new
                {
                    inputText = prompt,
                    textGenerationConfig = new
                    {
                        maxTokenCount = queryConfig.MaxTokens,
                        temperature = queryConfig.Temperature,
                        topP = queryConfig.TopP
                    }
                };
            }
            else if (queryConfig.ModelId.StartsWith("amazon.nova"))
            {
                requestBody = new
                {
                    messages = new[]
                    {
                            new
                            {
                                role = "user",
                                content = new[]
                                {
                                    new
                                    {
                                        text = prompt
                                    }
                                }
                            }
                        },
                    inferenceConfig = new
                    {
                        max_new_tokens = queryConfig.MaxTokens,
                        temperature = queryConfig.Temperature,
                        top_p = queryConfig.TopP,
                        top_k = queryConfig.TopK
                    }
                };
            }
            else if (queryConfig.ModelId.StartsWith("anthropic.claude"))
            {
                requestBody = new
                {
                    prompt = $"\n\nHuman: {prompt}\n\nAssistant:",
                    max_tokens_to_sample = queryConfig.MaxTokens,
                    temperature = queryConfig.Temperature,
                    top_p = queryConfig.TopP,
                    top_k = queryConfig.TopK
                };
            }
            else if (queryConfig.ModelId.StartsWith("ai21.jamba"))
            {
                requestBody = new
                {
                    messages = new[]
                    {
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_tokens = queryConfig.MaxTokens,
                    temperature = queryConfig.Temperature,
                    top_p = queryConfig.TopP
                };
            }
            else
            {
                throw new NotSupportedException($"Model '{queryConfig.ModelId}' is not supported.");
            }
            return requestBody;
        }

        private static string GetResponseOutput(string responseContent, string model)
        {

            var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

            // Parse response based on model type
            if (model.StartsWith("amazon.titan"))
            {
                if (responseObject.TryGetProperty("results", out var results) &&
                    results.GetArrayLength() > 0 &&
                    results[0].TryGetProperty("outputText", out var outputText))
                {
                    return outputText.GetString() ?? string.Empty;
                }
            }
            else if (model.StartsWith("amazon.nova"))
            {
                if (responseObject.TryGetProperty("output", out var output) &&
                    output.TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content) &&
                    content.GetArrayLength() > 0 &&
                    content[0].TryGetProperty("text", out var novaText))
                {
                    return novaText.GetString() ?? string.Empty;
                }
            }
            else if (model.StartsWith("anthropic.claude"))
            {
                if (responseObject.TryGetProperty("completion", out var completion))
                {
                    return completion.GetString() ?? string.Empty;
                }
            }
            else if (model.StartsWith("ai21.jamba"))
            {
                if (responseObject.TryGetProperty("choices", out var choices) &&
                    choices.GetArrayLength() > 0 &&
                    choices[0].TryGetProperty("message", out var message) &&
                    message.TryGetProperty("content", out var content))
                {
                    return content.GetString() ?? string.Empty;
                }
            }
            throw new Exception($"Model '{model}' is not supported for response parsing or the response is malformed.");
        }


    }
}
