﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;

namespace Framework.Caching.Redis.Protocol.Tests
{
    [TestClass]
    public class RespParserTest
    {
        [TestMethod]
        public void Parse_InvalidStart_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("|3\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'0'");
        }

        [TestMethod]
        public void Parse_EmptySimpleString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("+\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.SimpleString, response.ValueType);
            Assert.AreEqual(string.Empty, response.Value);
        }

        [TestMethod]
        public void Parse_SimpleString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("+foo\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.SimpleString, response.ValueType);
            Assert.AreEqual("foo", response.Value);
        }

        [TestMethod]
        public void Parse_SimpleStringWithInvalidEnd_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("+foo");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_SimpleStringWithInvalidEndSequence_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("+foo\r ");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_Error_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("-foo\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.Error, response.ValueType);
            Assert.AreEqual("foo", response.Value);
        }

        [TestMethod]
        public void Parse_ErrorWithInvalidEnd_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("-foo");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_ErrorWithInvalidEndSequence_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("-foo\r ");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_Integer_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes(":34\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(IntegerResponse));
            Assert.AreEqual(ValueType.Integer, response.ValueType);
            Assert.AreEqual(34L, response.Value);
        }

        [TestMethod]
        public void Parse_IntegerWithInvalidEnd_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes(":34");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_IntegerWithInvalidEndSequence_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes(":34\r ");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_BulkString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$3\r\nfoo\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, response.ValueType);
            Assert.AreEqual("foo", response.Value);
        }

        [TestMethod]
        public void Parse_BulkStringWithInvalidEnd_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$3\r\nfoo");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'7'");
        }

        [TestMethod]
        public void Parse_BulkStringWithInvalidEndSequence_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$3\r\nfoo\r ");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'7'");
        }

        [TestMethod]
        public void Parse_EmptyBulkString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$0\r\n\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, response.ValueType);
            Assert.AreEqual(string.Empty, response.Value);
        }

        [TestMethod]
        public void Parse_NullBulkString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$-1\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, response.ValueType);
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public void Parse_MultilineBulkString_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("$8\r\nfoo\r\nbar\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(StringResponse));
            Assert.AreEqual(ValueType.BulkString, response.ValueType);
            Assert.AreEqual("foo\r\nbar", response.Value);
        }

        [TestMethod]
        public void Parse_EmptyArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*0\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            Assert.AreEqual(0, ((object[])response.Value).Length);
        }

        [TestMethod]
        public void Parse_EmptyArrayWithInvalidEnd_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*0");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }


        [TestMethod]
        public void Parse_EmptyArrayWithInvalidEndSequence_Throws()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*0\r ");
            var target = new RespParser();

            var e = Assert.ThrowsException<ProtocolViolationException>(() => target.Parse(buffer).ToArray());
            StringAssert.Contains(e.Message, "'1'");
        }

        [TestMethod]
        public void Parse_NullArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*-1\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            Assert.IsNull(response.Value);
        }

        [TestMethod]
        public void Parse_IntegerArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*1\r\n:10\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            var result = (object[])response.Value;
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(10L, result[0]);
        }

        [TestMethod]
        public void Parse_BulkStringArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*2\r\n$3\r\nfoo\r\n$3\r\nbar\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            var result = (object[])response.Value;
            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("foo", result[0]);
            Assert.AreEqual("bar", result[1]);
        }

        [TestMethod]
        public void Parse_SimpleStringArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*1\r\n+foo\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            var result = (object[])response.Value;
            Assert.AreEqual(1, result.Length);
            Assert.AreEqual("foo", result[0]);
        }

        [TestMethod]
        public void Parse_MixedTypeArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*5\r\n:1\r\n:2\r\n:3\r\n:4\r\n$6\r\nfoobar\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            var result = (object[])response.Value;
            Assert.AreEqual(5, result.Length);
            Assert.AreEqual(1L, result[0]);
            Assert.AreEqual(2L, result[1]);
            Assert.AreEqual(3L, result[2]);
            Assert.AreEqual(4L, result[3]);
            Assert.AreEqual("foobar", result[4]);
        }

        [TestMethod]
        public void Parse_ArrayOfArrays_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*2\r\n*3\r\n:1\r\n:2\r\n:3\r\n*2\r\n+Foo\r\n-Bar\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);

            var result = (object[])response.Value;
            Assert.AreEqual(2, result.Length);

            var innerResult = (object[])result[0];
            Assert.AreEqual(1L, innerResult[0]);
            Assert.AreEqual(2L, innerResult[1]);
            Assert.AreEqual(3L, innerResult[2]);

            innerResult = (object[])result[1];
            Assert.AreEqual("Foo", innerResult[0]);
            Assert.AreEqual("Bar", innerResult[1]);
        }

        [TestMethod]
        public void Parse_NullElementsInArray_ReadValue()
        {
            var buffer = RespProtocol.Encoding.GetBytes("*3\r\n$3\r\nfoo\r\n$-1\r\n$3\r\nbar\r\n");
            var target = new RespParser();
            var responses = target.Parse(buffer);

            var response = responses.Single();
            Assert.IsInstanceOfType(response, typeof(ArrayResponse));
            Assert.AreEqual(ValueType.Array, response.ValueType);
            var result = (object[])response.Value;
            Assert.AreEqual(3, result.Length);
            Assert.AreEqual("foo", result[0]);
            Assert.IsNull(result[1]);
            Assert.AreEqual("bar", result[2]);
            Assert.AreEqual(ValueType.Array, response.ValueType);
        }
    }
}