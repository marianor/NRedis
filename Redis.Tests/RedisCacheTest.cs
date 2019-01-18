using Framework.Caching.Redis.Protocol;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
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
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");

            target.Set(expectedKey, expectedValue);

            IRequest request = null;
            var assert = new Func<IRequest, bool>(r =>
            {
                request = r;
                return r != null;
            });
            respClientMock.Verify(c => c.Execute(It.Is<IRequest>(r => assert(r))), Times.Once());
            Assert.AreEqual(2, request.GetArgs().Length);
            Assert.AreEqual("SET", request.Command);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(1));
        }

        // TODO need to more cases

        [TestMethod]
        public void Set_ValidKeyWithAbsoluteExpiration_InvokeExecute()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = DateTime.Now.AddHours(1);

            target.Set(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            IRequest request = null;
            var assert = new Func<IRequest, bool>(r =>
            {
                request = r;
                return r != null;
            });
            respClientMock.Verify(c => c.Execute(It.Is<IRequest>(r => assert(r))), Times.Once());
            Assert.AreEqual(3, request.GetArgs().Length);
            Assert.AreEqual("SET", request.Command);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(1));
            StringAssert.StartsWith(request.GetArg<string>(2), "PX ");
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
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");

            await target.SetAsync(expectedKey, expectedValue);

            IRequest request = null;
            var assert = new Func<IRequest, bool>(r =>
            {
                request = r;
                return r != null;
            });
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual(2, request.GetArgs().Length);
            Assert.AreEqual("SET", request.Command);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(1));
        }

        [TestMethod]
        public async Task SetAsync_ValidKeyWithAbsoluteExpiration_InvokeExecuteAsync()
        {
            var respClientMock = new Mock<IRespClient>();
            var target = new RedisCache(new RedisCacheOptions(), respClientMock.Object);
            var expectedKey = "foo";
            var expectedValue = RespProtocol.Encoding.GetBytes("bar");
            var expectedExpiration = DateTime.Now.AddHours(1);

            await target.SetAsync(expectedKey, expectedValue, new DistributedCacheEntryOptions { AbsoluteExpiration = expectedExpiration });

            IRequest request = null;
            var assert = new Func<IRequest, bool>(r =>
            {
                request = r;
                return r != null;
            });
            respClientMock.Verify(c => c.ExecuteAsync(It.Is<IRequest>(r => assert(r)), It.IsAny<CancellationToken>()), Times.Once());
            Assert.AreEqual(3, request.GetArgs().Length);
            Assert.AreEqual("SET", request.Command);
            Assert.AreEqual(expectedKey, request.GetArg<string>(0));
            CollectionAssert.AreEqual(expectedValue, request.GetArg<byte[]>(1));
            StringAssert.StartsWith(request.GetArg<string>(2), "PX ");
        }
        // TODO refresh
        // TODO Remove
    }
}