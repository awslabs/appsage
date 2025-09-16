using Microsoft.Extensions.Configuration;
namespace AppSage.Core.Configuration
{
    public class AppSageConfiguration : IAppSageConfiguration
    {
        IConfiguration _configuraiton;
        public AppSageConfiguration(IConfiguration configuration)
        {
            _configuraiton = configuration ?? throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null");
        }
        public string? this[string key]
        {
            get {

                return _configuraiton[key];
            }
        }

        public T Get<T>(string key)
        {
            var section = _configuraiton.GetSection(key);
            if (section.Exists())
            {
                return section.Get<T>();
            }
            throw new KeyNotFoundException($"Configuration key '{key}' not found.");
        }
    }
}
