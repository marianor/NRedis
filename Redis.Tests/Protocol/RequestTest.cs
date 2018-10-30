using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Framework.Caching.Protocol.Tests
{
    [TestClass]
    public class RequestTest
    {
        [TestMethod]
        public void RequestText_GetDatagram()
        {
            var target = new Request(RequestType.DBSize);

            Assert.AreEqual("DBSIZE\r\n", target.RequestText);
        }
    }
}