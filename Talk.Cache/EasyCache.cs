using System;
using System.Collections.Generic;
using System.Linq;

namespace Talk.Cache
{
    public class CacheData<T>
    {
        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpirationTime { get; set; }

        /// <summary>
        /// 要存储的数据
        /// </summary>
        public T Data { get; set; }
    }

    public class EasyCache<T>
    {
        public static Dictionary<string, CacheData<T>> list = new Dictionary<string, CacheData<T>>();

        public static object lockObj = new object();

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheData"></param>
        public void AddData(string key, CacheData<T> cacheData)
        {
            lock (lockObj)
            {
                list.Add(key, cacheData);
            }
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cacheData"></param>
        /// <returns></returns>
        public bool ModifyData(string key, CacheData<T> cacheData)
        {
            var keys = list.Where(t => t.Value.ExpirationTime <= DateTime.Now).Select(t => t.Key).ToArray();
            lock (lockObj)
            {
                foreach (var k in keys)
                {
                    list.Remove(k);
                }
                if (!list.ContainsKey(key))
                    return false;
                list[key] = cacheData;
                return true;
            }
        }

        /// <summary>
        /// 如果没有取到 则返回null
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public CacheData<T> GetData(string key)
        {
            var keys = list.Where(t => t.Value.ExpirationTime <= DateTime.Now).Select(t => t.Key).ToArray();
            lock (lockObj)
            {
                foreach (var k in keys)
                {
                    list.Remove(k);
                }
                if (list.ContainsKey(key))
                    return list[key];
                return null;
            }
        }

    }

    public class ExecuteIndex
    {
        private EasyCache<long> easy;
        public string _key;
        public ExecuteIndex(string key, DateTime time)
        {
            _key = key;
            easy = new EasyCache<long>();
            easy.AddData(key, new CacheData<long>()
            {
                Data = 0,
                ExpirationTime = time,
            });
        }

        public long GetIndex()
        {
            easy.ModifyData(_key, new CacheData<long>()
            {
                Data = easy.GetData(_key).Data + 1,
                ExpirationTime = easy.GetData(_key).ExpirationTime,
            });
            return easy.GetData(_key).Data;
        }
    }
}
