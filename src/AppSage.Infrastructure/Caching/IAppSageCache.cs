namespace AppSage.Infrastructure.Caching
{
    public interface IAppSageCache
    {
        public void Set(string key, string value);
        public string Get(string key);
        public void Remove(string key);
        public bool ContainsKey(string key);
        public void Clear();
    }
}
