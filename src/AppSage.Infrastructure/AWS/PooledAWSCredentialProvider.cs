using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AppSage.Core.Configuration;
using AppSage.Core.Logging;

namespace AppSage.Infrastructure.AWS
{
    public class PooledAWSCredentialProvider : IAWSCredentialProvider
    {
        private long _currentIndex = 0;
        IAppSageLogger _logger;
        IAppSageConfiguration _config;
        public PooledAWSCredentialProvider(IAppSageLogger logger, IAppSageConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null. Please provide a valid logger instance.");
            _config = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null. Please provide a valid configuration instance.");
        }
        public AWSCredentials GetCredentials()
        {

            AWSCredentials? credentials = null;
            string[] profileNameList = _config.Get<string[]>("AppSage.Infrastructure.AWS:CredentialProfileNames");
            try
            {
                var profileChain = new CredentialProfileStoreChain();

                if (profileNameList == null || profileNameList.Length == 0)
                {
                    //If no profile name is given, fall back to default credential chain on the running machine
                    credentials = new EnvironmentVariablesAWSCredentials();
                }
                else
                {
                    // Rotate through the provided profile names for each request
                    Interlocked.Increment(ref _currentIndex);
                    string profileName = profileNameList[_currentIndex % profileNameList.Length];
                    if (!profileChain.TryGetAWSCredentials(profileName, out credentials))
                    {
                        // If the credential profile is invalid or non existing, fallback to environment variables and default credential chain
                        credentials = new EnvironmentVariablesAWSCredentials();
                    }
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
