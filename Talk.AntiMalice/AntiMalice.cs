using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Net;
using Talk.Cache;

namespace Talk.AntiMalice
{
    /// <summary>
    ///反捣乱
    /// </summary>
    public static class AntiMalice
    {
        //private readonly static string defaultValue = "defaltValue";
        private readonly static string key = "talkantimaliceXX";

        /// <summary>
        /// 设置Token
        /// </summary>
        /// <param name="httpContext"></param>
        public static void SetToken(this HttpContext httpContext)
        {
            var antimaliceValue = httpContext.Request.Cookies.FirstOrDefault(t => t.Key == key).Value;
            if (string.IsNullOrWhiteSpace(antimaliceValue))//如果没有cookie
            {
                var value = "Talke|" + DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                value = value.DES3Encrypt(key);
                httpContext.Response.Cookies.Append(key, value);
            }
        }
        /// <summary>
        /// 验证Token
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="interval">时间间隔</param>
        /// <param name="nsecond">未标记请求请求可以访问次数</param>
        /// <param name="usecond">标记请求可以访问次数（两分钟后清除标记）</param>
        /// <returns></returns>
        public static bool ValidateToken(this HttpContext httpContext, int interval = 30, int nsecond = 15, int usecond = 5)
        {
            var antimaliceValue = httpContext.Request.Cookies.FirstOrDefault(t => t.Key == key).Value;
            if (string.IsNullOrWhiteSpace(antimaliceValue))//如果没有cookie           
                return false;
            var time = DateTime.Now.AddSeconds(interval) - DateTime.Now;
            ExecuteNum num = new ExecuteNum(antimaliceValue, time);
            if (string.IsNullOrWhiteSpace(antimaliceValue))//证明是没有标记的请求
            {
                if (num.GetNum() >= nsecond)//一定时间内不能超过多少次
                    return false;
            }
            else
            {
                if (num.GetNum() >= usecond)//有标记也不能频繁操作
                    return false;
                var valueString = antimaliceValue.DES3Decrypt(key);
                var values = valueString.Split('|');
                if (values.Length != 2 || values[0] != "Talke")
                    return false;
                else if (DateTime.Parse(values[1]).AddMinutes(2) <= DateTime.Now)//清除2分钟前的标记                
                    httpContext.Response.Cookies.Delete(key);
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <param name="chanceNum">随机通过概率</param>
        /// <returns></returns>
        public static bool GetChance(this HttpContext httpContext, int chanceNum = 2)
        {
            var isAntiMalice = httpContext.ValidateToken();
            if (!isAntiMalice)
                return new Random().Next(100000) % chanceNum == 0;
            return true;
        }
    }
}
