using Amazon.Runtime;

namespace AppSage.Infrastructure
{
    public interface IAWSCredentialProvider
    {
        AWSCredentials GetCredentials();
    }
}
