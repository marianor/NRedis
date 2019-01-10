using Framework.Caching.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace Framework.Caching.Tests
{
    [TestClass]
    public class RedisCacheTest
    {
        [TestMethod]
        public void RedisCache_OptionsAccessorIsNull_Throws()
        {
            RedisCacheOptions optionsAccessor = null;

            var e = Assert.ThrowsException<ArgumentNullException>(() => new RedisCache(optionsAccessor));
            Assert.AreEqual("optionsAccessor", e.ParamName);
        }

        [TestMethod]
        public void Get_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions { Host = "foo" });

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Get(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Get_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Get(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.GetAsync(null));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task GetAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.GetAsync(string.Empty));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Set_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Set(null, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public void Set_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = Assert.ThrowsException<ArgumentException>(() => target.Set(string.Empty, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task SetAsync_KeyIsNull_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.SetAsync(null, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        [TestMethod]
        public async Task SetAsync_KeyIsEmpty_Throws()
        {
            var target = new RedisCache(new RedisCacheOptions(), Mock.Of<IRespClient>());

            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.SetAsync(string.Empty, new byte[] { 65 }, new DistributedCacheEntryOptions()));
            Assert.AreEqual(e.ParamName, "key");
        }

        // TODO refresh
        // TODO Remove
    }
}