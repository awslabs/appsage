using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AppSage.Core.Logging;
using AppSage.Infrastructure;

namespace AppSage.Run
{
    internal class AWSCredentialProvider:IAWSCredentialProvider
    {
        private const string DEFAULT_PROFILE = "appsage-profile";
        IAppSageLogger _logger;
        public AWSCredentialProvider(IAppSageLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null. Please provide a valid logger instance.");
        }
        public AWSCredentials GetCredentials()
        {
             
            // Set the AWS_PROFILE environment variable for this process. For the development purpose only. 
            //Environment.SetEnvironmentVariable("AWS_PROFILE", DEFAULT_PROFILE, EnvironmentVariableTarget.Process);
             
            AWSCredentials? credentials = null;

            try
            {
                // Attempt to retrieve AWS credentials using the credential management store
                var profileChain = new CredentialProfileStoreChain();
                if (!profileChain.TryGetAWSCredentials(DEFAULT_PROFILE, out credentials))
                {
                    // Fallback to environment variables and default credential chain
                    credentials = new EnvironmentVariablesAWSCredentials();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving AWS credentials: {ex.Message}");
                throw ex;
            }
            return credentials;
        }
    }
}
