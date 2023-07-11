namespace AccountManagement.Application.Redis.Common
{
    public interface IRedisProvider
    {
        public List<string> GetList(string key);
        public long PushTopIfNotExist(string key, string value);
        public bool SetValueByKey(string key, string value, int time = 0);
        public string DeleteByKey(string key);
        public string GetByKey(string key);

    }
}
