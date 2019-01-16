using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Tests
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

        // TODO test features of Get & GetAsync

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
        public void Set_ValidKey_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            const string expectedKey = "foo";
            const string expectedValue = "bar";

            target.Set(expectedKey, RespProtocol.Encoding.GetBytes(expectedValue));

            var assert = new Func<IEnumerable<IRequest>, bool>(requests =>
            {
                var request = (KeyValueRequest)requests.Single();
                return request.Command == "SET" && request.Key == expectedKey && request.Value == expectedValue;
            });
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))));
        }

        [TestMethod]
        public void Set_ValidKeyWithAbsoluteExpiration_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            const string expectedKey = "foo";
            const string expectedValue = "bar";
            var expectedExpiration = DateTime.Now.AddHours(1);

            target.Set(expectedKey, RespProtocol.Encoding.GetBytes(expectedValue), new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            var assert = new Func<IEnumerable<IRequest>, bool>(requests =>
            {
                var keyValue = (KeyValueRequest)requests.First();
                var expireAt = (PExpireAtRequest)requests.ElementAt(1);
                return requests.Count() == 2
                    && keyValue.Command == "SET"
                    && keyValue.Key == expectedKey
                    && keyValue.Value == expectedValue
                    && expireAt.Command == "PEXPIREAT"
                    && expireAt.Key == expectedKey
                    && expireAt.AbsoluteExpiration == expectedExpiration;
            });
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))));
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

        [TestMethod]
        public async Task SetAsync_ValidKey_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            const string expectedKey = "foo";
            const string expectedValue = "bar";

            await target.SetAsync(expectedKey, RespProtocol.Encoding.GetBytes(expectedValue));

            var assert = new Func<IEnumerable<IRequest>, bool>(requests =>
            {
                var request = (KeyValueRequest)requests.Single();
                return request.Command == "SET"
                    && request.Key == expectedKey
                    && request.Value == expectedValue;
            });
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IEnumerable<IRequest>>(r => assert(r)), It.IsAny<CancellationToken>()));
        }

        [TestMethod]
        public async Task SetAsync_ValidKeyWithAbsoluteExpiration_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            const string expectedKey = "foo";
            const string expectedValue = "bar";
            var expectedExpiration = DateTime.Now.AddHours(1);

            await target.SetAsync(expectedKey, RespProtocol.Encoding.GetBytes(expectedValue), new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            var assert = new Func<IEnumerable<IRequest>, bool>(requests =>
            {
                var keyValue = (KeyValueRequest)requests.First();
                var expireAt = (PExpireAtRequest)requests.ElementAt(1);
                return requests.Count() == 2
                    && keyValue.Command == "SET"
                    && keyValue.Key == expectedKey
                    && keyValue.Value == expectedValue
                    && expireAt.Command == "PEXPIREAT"
                    && expireAt.Key == expectedKey
                    && expireAt.AbsoluteExpiration == expectedExpiration;
            });
            respClientMock.Verify(c => c.Execute(It.Is<IEnumerable<IRequest>>(r => assert(r))));
        }
        // TODO refresh
        // TODO Remove
    }
}