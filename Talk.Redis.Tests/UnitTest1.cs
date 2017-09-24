using System;
using System.Threading.Tasks;
using Xunit;

namespace Talk.Redis.Tests
{
    public class UnitTest1
    {
        public UnitTest1()
        {
            RedisHelper.RedisConnection = "127.0.0.1:6379,allowAdmin=true,password=haojimaRedis";
        }
        [Fact]
        public async Task Test1Async()
        {
            RedisHelper redis = new RedisHelper(1);
            await redis.SetStringAsync("key3", "hahah");
            var value = await redis.GetStringAsync("key3");
            Assert.True(value == "hahah");
        }

        [Fact]
        public async Task Test2Async()
        {
            RedisHelper redis = new RedisHelper(3);
            await redis.SetStringAsync("key5", "hahah");
            var value = await redis.GetStringAsync("key5");
            Assert.True(value == "hahah");
        }
    }
}
