using Framework.Caching.Protocol;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Tests
{
    [TestClass]
    public class RedisCacheTest
    {
        [TestMethod]
        public void RedisCache_ClientIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new RedisCache(null));
            Assert.AreEqual("client", e.ParamName);
        }

        [TestMethod]
        public void Get_KeyIsNull_Throws()
        {
            var respClient = Moq.Mock.Of<IRespClient>();
            var target = new RedisCache(respClient);

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Get(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Get_KeyIsEmpty_Throws()
        {
            var respClient = Moq.Mock.Of<IRespClient>();
            var target = new RedisCache(respClient);

            var e = Assert.ThrowsException<ArgumentException>(() => target.Get(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_KeyIsNull_Throws()
        {
            var respClient = Moq.Mock.Of<IRespClient>();
            var target = new RedisCache(respClient);

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.GetAsync(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_KeyIsEmpty_Throws()
        {
            var respClient = Moq.Mock.Of<IRespClient>();
            var target = new RedisCache(respClient);

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.GetAsync(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }
    }
}