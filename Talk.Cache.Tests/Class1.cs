using System;
using Xunit;

namespace Talk.Cache.Tests
{

    public class Class1
    {

        [Fact]
        public void test()
        {
            //  Assert.Equal(obj2.Name, "张三"); 
            EasyCache<string> a = new EasyCache<string>();
            a.AddData("k", new CacheData<string>()
            {
                ExpirationTime = DateTime.Now.AddDays(1),
                Data = "aaa",
            });

            a.AddData("j", new CacheData<string>()
            {
                ExpirationTime = DateTime.Now.AddDays(1),
                Data = "ccc",
            });
            var vvv = a.GetData("k");
            Assert.Equal(a.GetData("k")?.Data, "aaa");
            Assert.Equal(a.GetData("j")?.Data, "ccc");

            EasyCache<string> z = new EasyCache<string>();
            Assert.Equal(z.GetData("k")?.Data, "aaa");

            EasyCache<int> b = new EasyCache<int>();
            b.AddData("b", new CacheData<int>()
            {
                ExpirationTime = DateTime.Now.AddDays(1),
                Data = 111,
            });

            Assert.Equal(b.GetData("b")?.Data, 111);

            EasyCache<int> index = new EasyCache<int>();
            index.AddData("v", new CacheData<int>()
            {
                ExpirationTime = DateTime.Now.AddDays(1),
                Data = 1,
            });

            ExecuteIndex e = new ExecuteIndex("ind", DateTime.Now.AddDays(2));
            Assert.Equal(e.GetIndex(), 1);
            Assert.Equal(e.GetIndex(), 2);
            Assert.Equal(e.GetIndex(), 3);
        }

    }
}
