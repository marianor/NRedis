using Framework.Caching.Transport;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class RespClientTest
    {
        [TestMethod]
        public void RespClient_SettingsIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new RespClient(null));

            Assert.AreEqual(e.ParamName, "settings");
        }

        [TestMethod]
        public void Execute_RequestIsNull_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            IRequest request = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public void Execute_RequestsIsNull_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            IEnumerable<IRequest> requests = null;
            var e = Assert.ThrowsException<ArgumentNullException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public void Execute_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            var requests = Enumerable.Empty<IRequest>();
            var e = Assert.ThrowsException<ArgumentException>(() => target.Execute(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestIsNull_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            IRequest request = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(request));

            Assert.AreEqual(e.ParamName, "request");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsNull_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            IEnumerable<IRequest> requests = null;
            var e = await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }

        [TestMethod]
        public async Task ExecuteAsync_RequestsIsEmpty_Throws()
        {
            var target = new RespClient(Mock.Of<ITransportSettings>());
            var requests = Enumerable.Empty<IRequest>();
            var e = await Assert.ThrowsExceptionAsync<ArgumentException>(() => target.ExecuteAsync(requests));

            Assert.AreEqual(e.ParamName, "requests");
        }
    }
}