using NRedis.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NRedis.Tests
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
        public void AddDistributedRedisCache_Valid_ExecutesSetupAction()
        {
            var descriptors = new List<ServiceDescriptor>();
            var servicesMock = new Mock<IServiceCollection>();
            servicesMock.Setup(x => x.GetEnumerator()).Returns(() => descriptors.GetEnumerator());
            servicesMock.Setup(x => x.Add(It.IsAny<ServiceDescriptor>())).Callback<ServiceDescriptor>(s => descriptors.Add(s));

            servicesMock.Object.AddDistributedRedisCache(o => { });

            var descriptor = descriptors.Single(d => d.ServiceType == typeof(IDistributedCache));
            Assert.AreEqual(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.AreEqual(typeof(RedisCache), descriptor.ImplementationType);
        }
    }
}