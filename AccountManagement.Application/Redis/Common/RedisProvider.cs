using AccountManagement.Infrastructure.Database;
using System.Text;

namespace AccountManagement.Application.Redis.Common
{
    public class RedisProvider : IRedisProvider
    {
        /// <summary>
        /// Lấy danh sách user theo key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public List<string> GetList(string key)
        {
            var rs = new List<string>();
            try
            {
                var list = RedisDb.ListRange(key);
                foreach (var item in list)
                {
                    rs.Add(string.Concat(item));
                }
            }
            catch 
            {
                rs = null;
            }
            
            return rs;
        }

        /// <summary>
        /// Thêm account vào đầu danh sách.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public long PushTopIfNotExist(string key, string value)
        {
            var rs = (long)-1.0;
            try
            {
                if (string.IsNullOrEmpty(value)) return rs;
                rs = RedisDb.ListLeftPush(key, value.ToUpper());
            }
            catch
            {
            }
            
            return rs;
        }

        /// <summary>
        /// Thêm account vào đầu danh sách.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool SetValueByKey(string key, string value, int time = 0)
        {
            return RedisDb.StringSet(key, value, time);
        }

        /// <summary>
        /// Xóa key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string DeleteByKey(string key)
        {
            var rs = "";
            try
            {
                rs = RedisDb.KeyDelete(key).ToString();
            }
            catch (Exception ex)
            {
                rs = null;
            }
            
            return rs;
        }

        public string GetByKey(string key)
        {
            try
            {
                return RedisDb.StringGet(key);
            }
            catch
            {
                return null;
            }
        }
    }
}
