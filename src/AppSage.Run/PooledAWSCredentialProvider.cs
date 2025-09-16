using Amazon.Runtime;
using Amazon.Runtime.CredentialManagement;
using AppSage.Core.Logging;
using AppSage.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSage.Run
{

    internal class PooledAWSCredentialProvider : IAWSCredentialProvider
    {
        // Used to avoid throttling issues with a single AWS account. This will roundrobbin aws accounts (for Bedrock queries). Use single entry if you want to use one aws account
        private string[] _profileList = ["appsage-profile", "appsage-profile-2", "appsage-profile-3"];
        private long _currentIndex = 0;
        IAppSageLogger _logger;
        public PooledAWSCredentialProvider(IAppSageLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger cannot be null. Please provide a valid logger instance.");
        }
        public AWSCredentials GetCredentials()
        {

            AWSCredentials? credentials = null;

            try
            {
                System.Threading.Interlocked.Increment(ref _currentIndex);
                string profileName = _profileList[_currentIndex % _profileList.Length]; 
                // Attempt to retrieve AWS credentials using the credential management store
                var profileChain = new CredentialProfileStoreChain();
                if (!profileChain.TryGetAWSCredentials(profileName, out credentials))
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
