using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
    }
}