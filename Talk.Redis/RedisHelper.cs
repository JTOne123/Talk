using StackExchange.Redis;
using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;

namespace Talk.Redis
{
    public class RedisHelper
    {
        /// <summary>
        /// ConnectionMultiplexer是线程安全的，且是昂贵的。所以我们尽量重用。 
        /// </summary>
        private static ConnectionMultiplexer connectionMultiplexer;
        private IDatabase database { get; set; }
        public static string RedisConnection { get; set; }

        public RedisHelper(int db)
        {
            if (string.IsNullOrWhiteSpace(RedisConnection))
                throw new Exception("没有配置redis连接");
            if (connectionMultiplexer == null)
                connectionMultiplexer = ConnectionMultiplexer.Connect(RedisConnection);
            database = connectionMultiplexer.GetDatabase(db);
        }

        /// <summary>
        /// 存储字符串
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry">有效期</param>
        /// <returns></returns>
        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                await database.StringSetAsync(RedisTypePrefix.String.GetDescription() + key, value, expiry);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// 读取字符串
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<string> GetStringAsync(string key)
        {
            try
            {
                return await database.StringGetAsync(RedisTypePrefix.String.GetDescription() + key);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// 计数器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry">只有第一次设置有效期生效</param>
        /// <returns></returns>
        public async Task<long> SetStringIncrAsync(string key, long value, TimeSpan? expiry = null)
        {
            try
            {
                key = RedisTypePrefix.String.GetDescription() + key;

                var nubmer = await database.StringIncrementAsync(key, value);
                if (nubmer == 1 && expiry != null)//只有第一次设置有效期（防止覆盖）
                    await database.KeyExpireAsync(key, expiry);//设置有效期
                return nubmer;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }

        /// <summary>
        /// 计数器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="expiry"></param>
        /// <returns></returns>
        public async Task<long> SetStringIncrAsync(string key, TimeSpan? expiry = null)
        {
            return await SetStringIncrAsync(key, 1, expiry);
        }

        /// <summary>
        /// 读取计数器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<long> GetStringIncrAsync(string key)
        {
            var value = await GetStringAsync(key);
            if (string.IsNullOrWhiteSpace(value))
            {
                return 0;
            }
            else
            {
                return long.Parse(value);
            }
        }

        /// <summary>
        /// 判断key是否存在
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> KeyExistsAsync(string key, RedisTypePrefix redisTypePrefix)
        {
            return await database.KeyExistsAsync(redisTypePrefix.GetDescription() + key);
        }

        /// <summary>
        /// 删除key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> DeleteKeyAsync(string key, RedisTypePrefix redisTypePrefix)
        {
            return await database.KeyDeleteAsync(redisTypePrefix.GetDescription() + key);
        }
    }

    #region help
    public static class RedisEnumExtension
    {
        /// <summary>
        ///  获取枚举的中文描述
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetDescription(this Enum obj)
        {
            string objName = obj.ToString();
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(objName);
            DescriptionAttribute[] arrDesc = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return arrDesc[0].Description;
        }
    }

    #endregion
}
