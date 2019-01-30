using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void Request_CommandIsNull_Throws()
        {
            var e = Assert.ThrowsException<ArgumentNullException>(() => new Request(null));

            Assert.AreEqual("command", e.ParamName);
        }

        [TestMethod]
        public void Request_CommandIsEmpty_Throws()
        {
            var e = Assert.ThrowsException<ArgumentException>(() => new Request(string.Empty));

            Assert.AreEqual("command", e.ParamName);
        }
    }
}