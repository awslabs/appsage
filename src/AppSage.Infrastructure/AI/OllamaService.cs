using AppSage.Core.Configuration;
using AppSage.Core.Logging;
using AppSage.Infrastructure.Caching;
using SharpToken;
using System.Text;
using System.Text.Json;

namespace AppSage.Infrastructure.AI
{
    public class OllamaService : IAIQuery
    {
        private readonly IAppSageConfiguration _configuration;
        private readonly IAppSageLogger _logger;
        private readonly IAppSageCache _cache;

        public OllamaService(IAppSageConfiguration configuration, IAppSageLogger logger, IAppSageCache cache)
        {
            _configuration = configuration;
            _logger = logger;
            _cache = cache;
            var baseUrl = _configuration.Get<string>("AppSage.Infrastructure.AI.OllamaService:BaseUrl");
        }

        public static class Models
        {
            public const string DeepSeekV2 = "deepseek-coder-v2:16b";
            public const string Qwen2Coder = "qwen2.5-coder:1.5b-base";
        }

        public string Invoke(string prompt)
        {


            var queryConfig = new AIQueryConfig();

            //Estimate prompt size uisng gpt -3.5-turbo encoding. This is an approximation good for most models
            var encoding = GptEncoding.GetEncodingForModel("gpt-3.5-turbo");

            // Tokenize and count
            int tokenCount = encoding.Encode(prompt).Count;

            _logger.LogInformation($"OllamaService: Estimated token count for prompt is {tokenCount} tokens.");

            //default model;
            queryConfig.ModelId = _configuration.Get<string>("AppSage.Infrastructure.AI.OllamaService:ModelId");
            switch (tokenCount)
            {
                case < 5000:
                    queryConfig.ModelId = Models.DeepSeekV2;
                    break;
                case < 200000:
                    queryConfig.ModelId = Models.DeepSeekV2;
                    break;
                default:
                    queryConfig.ModelId = Models.DeepSeekV2;
                    break;
            }

            queryConfig.MaxTokens = _configuration.Get<int>("AppSage.Infrastructure.AI.OllamaService:MaxTokens");
            queryConfig.Temperature = _configuration.Get<double>("AppSage.Infrastructure.AI.OllamaService:Temperature");
            queryConfig.TopK = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:TopK");
            queryConfig.TopP = _configuration.Get<int>("AppSage.Infrastructure.AI.Bedrock:TopP");
            return Invoke(prompt, queryConfig);
        }



        public string Invoke(string prompt, AIQueryConfig queryConfig)
        {
            string cacheKey = Utility.GenerateCacheKey(prompt, queryConfig);
            if (_cache.ContainsKey(cacheKey))
            {
                _logger.LogInformation($"OllamaService: Cache hit for key {cacheKey}");
                return _cache.Get(cacheKey);
            }
            else
            {
                // Read configuration values - using indexer syntax since GetValue method is not available
                var baseUrl = _configuration.Get<string>("AppSage.Infrastructure.AI.OllamaService:BaseUrl");
                if (string.IsNullOrEmpty(baseUrl))
                {
                    throw new InvalidOperationException("Ollama BaseUrl configuration is missing or empty");
                }

                var timeoutString = _configuration.Get<string>("AppSage.Infrastructure.AI.OllamaService:Timeout");
                if (!int.TryParse(timeoutString, out int timeout))
                {
                    timeout = 300; // Default to 5min
                }

                // Initialize HttpClient
                using (HttpClient httpClient = new HttpClient())
                {
                    // Ensure baseUrl doesn't have trailing slash to avoid double slashes
                    var normalizedBaseUrl = baseUrl.TrimEnd('/');
                    httpClient.Timeout = TimeSpan.FromSeconds(timeout);

                    var requestPayload = GetRequestBody(prompt, queryConfig);

                    var content = new StringContent(requestPayload, Encoding.UTF8, "application/json");

                    try
                    {
                        // Use the full URL to avoid potential base URL issues
                        var fullUrl = $"{normalizedBaseUrl}/api/generate";
                        var response = httpClient.PostAsync(fullUrl, content).Result;

                        response.EnsureSuccessStatusCode();

                        var responseContent = response.Content.ReadAsStringAsync().Result;
                        var responseObject = JsonSerializer.Deserialize<JsonElement>(responseContent);


                        if (responseObject.TryGetProperty("response", out var responseProperty))
                        {
                            string result = responseProperty.GetString() ?? string.Empty;
                            if (!String.IsNullOrEmpty(result))
                            {
                                _cache.Set(cacheKey, result);
                            }
                            else
                            {
                                _logger.LogWarning($"OllamaService: Response is empty for prompt: {prompt}");
                            }

                            return result;
                        }
                        else
                        {
                            throw new InvalidDataException("Response does not contain 'response' property.");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"Failed to invoke Ollama model: {ex.Message}", ex);
                    }
                }
            }
        }
    
    public static string GetRequestBody(string prompt, AIQueryConfig queryConfig)
    {
        object requestPayload;

        // Create request body based on model type
        if (queryConfig.ModelId.StartsWith("deepseek-coder"))
        {
            // DeepSeek Coder models support advanced parameters
            requestPayload = new
            {
                model = queryConfig.ModelId,
                prompt = prompt,
                stream = false,
                options = new
                {
                    num_predict = queryConfig.MaxTokens,
                    temperature = queryConfig.Temperature,
                    top_k = queryConfig.TopK,
                    top_p = queryConfig.TopP,
                    repeat_penalty = 1.1,
                    stop = new[] { "\n\n", "```", "Human:", "Assistant:" }
                }
            };
        }
        else if (queryConfig.ModelId.StartsWith("qwen2.5-coder"))
        {
            // Qwen2.5 Coder models - optimized for code generation
            requestPayload = new
            {
                model = queryConfig.ModelId,
                prompt = prompt,
                stream = false,
                options = new
                {
                    num_predict = queryConfig.MaxTokens,
                    temperature = queryConfig.Temperature,
                    top_k = queryConfig.TopK,
                    top_p = queryConfig.TopP,
                    repeat_penalty = 1.05,
                    stop = new[] { "\n\n\n", "```\n" }
                }
            };
        }
        else
        {
            // Default request body for other models
            requestPayload = new
            {
                model = queryConfig.ModelId,
                prompt = prompt,
                stream = false,
                options = new
                {
                    num_predict = queryConfig.MaxTokens,
                    temperature = queryConfig.Temperature,
                    top_k = queryConfig.TopK,
                    top_p = queryConfig.TopP
                }
            };
        }

        return JsonSerializer.Serialize(requestPayload);
    }
    }
}
