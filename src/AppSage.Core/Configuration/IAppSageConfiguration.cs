namespace AppSage.Core.Configuration
{
    public interface IAppSageConfiguration
    {
        public bool KeyExist(string key);
        public T Get<T>(string key);
    }

    public interface IAppSageConfigurationWriter
    {
  
        public void Set<T>(string key, T value);
    }
}
