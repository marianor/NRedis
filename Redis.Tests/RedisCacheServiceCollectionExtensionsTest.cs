using Framework.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Framework.Caching.Tests
{
    [TestClass]
    public class RedisCacheServiceCollectionExtensionsTest
    {
        [TestMethod]
        public void AddDistributedRedisCache_ServicesIsNull_Throws()
        {
            IServiceCollection services = null;

            var e = Assert.ThrowsException<ArgumentNullException>(() => services.AddDistributedRedisCache(s => { }));
            Assert.AreEqual("services", e.ParamName);
        }

        [TestMethod]
        public void AddDistributedRedisCache_SetupActionIsNull_Throws()
        {
            var services = Mock.Of<IServiceCollection>();

            var e = Assert.ThrowsException<ArgumentNullException>(() => services.AddDistributedRedisCache(null));
            Assert.AreEqual("setupAction", e.ParamName);
        }

        [TestMethod]
        public void AddDistributedRedisCache_1SetupActionIsNull_Throws()
        {
            var services = Mock.Of<IServiceCollection>();

            var e = Assert.ThrowsException<ArgumentNullException>(() => services.AddDistributedRedisCache(null));
            Assert.AreEqual("setupAction", e.ParamName);
        }
    }
}