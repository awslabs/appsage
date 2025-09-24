using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AppSage.Core.Configuration;
using AppSage.Core.Logging;

namespace AppSage.Infrastructure.AWS
{
    public class AWSCredentialProvider:IAWSCredentialProvider
    {
        IAppSageLogger _logger;
        IAppSageConfiguration _config;
        public AWSCredentialProvider(IAppSageLogger logger,IAppSageConfiguration config)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null. Please provide a valid logger instance.");
            _config = config ?? throw new ArgumentNullException(nameof(config), "Configuration cannot be null. Please provide a valid configuration instance.");
        }
        public AWSCredentials GetCredentials()
        {             
            AWSCredentials? credentials = null;

            try
            {
                string[] profileNameList = _config.Get<string[]>("AppSage.Infrastructure.AWS:CredentialProfileNames");

                // Attempt to retrieve AWS credentials using the credential management store
                var profileChain = new CredentialProfileStoreChain();
                
                if(profileNameList == null || profileNameList.Length == 0)
                {
                    //If no profile name is given, fall back to default credential chain on the running machine
                    credentials = new EnvironmentVariablesAWSCredentials();
                }
                else if (!profileChain.TryGetAWSCredentials(profileNameList[0], out credentials)) 
                {
                    // If the credential profile is invalid or non existing, fallback to environment variables and default credential chain
                    credentials = new EnvironmentVariablesAWSCredentials();
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving AWS credentials: {ErrorMessage}", ex.Message);
                throw ex;
            }
            return credentials;
        }
    }
}
