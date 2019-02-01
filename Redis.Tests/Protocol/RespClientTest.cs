using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RespClientTest
    {
        [TestMethod]
        public void RespClient_OptionsAccessorIsNull_Throws()
        {
            IOptions<RedisCacheOptions> optionsAccessor = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => new RespClient(optionsAccessor));

            Assert.AreEqual(e.ParamName, "optionsAccessor");
        }

        [TestMethod]
        public void Execute_RequestIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest request = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public void Execute_RequestsIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest[] requests = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            var requests = Array.Empty<IRequest>();
            var e = Assert.ThrowsException<ArgumentException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest request = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsNull_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            IRequest[] requests = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(new RedisCacheOptions { Host = "foo" });
            var requests = Array.Empty<IRequest>();
            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }
    }
}