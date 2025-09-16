namespace AppSage.Core.Configuration
{
    public interface IAppSageConfiguration
    {
        public T Get<T>(string key);
    }
}
