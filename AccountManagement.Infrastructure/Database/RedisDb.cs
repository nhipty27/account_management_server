using StackExchange.Redis;
using System.Text.Json;

namespace AccountManagement.Infrastructure.Database
{
    public class RedisDb
    {

        /// <summary>
        /// StringSet
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="time"></param>
        public static bool StringSet(string key, string value, int time)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase _db = redis.GetDatabase();
            var timeSpan = TimeSpan.FromDays(time);
            return _db.StringSet(key, value, timeSpan);
        }

        /// <summary>
        /// StringGet
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string StringGet(string key)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase _db = redis.GetDatabase();
            if(_db.KeyExists(key))
                return _db.StringGet(key).ToString();
            return null;
        }

        /// <summary>
        /// KeyDelete
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool KeyDelete(string key)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase _db = redis.GetDatabase();
            return _db.KeyDelete(key);
        }

        /// <summary>
        /// ListRange
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<RedisValue> ListRange(string key)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase _db = redis.GetDatabase();
            return _db.ListRange(key).ToList();
        }

        /// <summary>
        /// ListLeftPush
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static long ListLeftPush(string key, string value)
        {
            var redis = ConnectionMultiplexer.Connect("localhost:6379");
            IDatabase _db = redis.GetDatabase();
            return _db.ListLeftPush(key, value);
        }

    }
}
