using Framework.Caching.Properties;
using Framework.Caching.Transport;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Framework.Caching.Protocol
{
    // Ver http://redis.io/topics/protocol
    public class RespClient : IRespClient
    {
        internal const char CR = '\r';
        internal const char LF = '\n';

        private const char SimpleString = '+';
        private const char Error = '-';
        private const char Integer = ':';
        private const char BulkString = '$';
        private const char Array = '*';

        private readonly ITransport _transport;
        private readonly ITransportSettings _settings;

        public RespClient(ITransportSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _transport = settings.CreateTransport();
        }

        public IResponse[] Execute(params IRequest[] requests)
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            Connect();
            var request = string.Concat(requests.Select(c => c.RequestText));
            var responseText = _transport.Send(request);
            return new RespParser(responseText).Parse();
        }

        private void Connect()
        {
            if (_transport.State == TransportState.Closed)
            {
                _transport.Connect();
                if (_settings.Password != null)
                {
                    var response = Execute(new KeyRequest(RequestType.Auth, _settings.Password));
                    if (!Equals(response[0].Value, "OK"))
                        throw new AuthenticationException("");
                }
            }
        }

        public async Task<IResponse[]> ExecuteAsync(IRequest[] requests, CancellationToken token = default(CancellationToken))
        {
            if (requests == null)
                throw new ArgumentNullException(nameof(requests));
            if (requests.Length == 0)
                throw new ArgumentException(Resources.ArgumentCannotBeEmpty, nameof(requests));

            await ConnectAsync(token);

            var request = string.Concat(requests.Select(c => c.RequestText));
            var responseText = await _transport.SendAsync(request, token);
            return new RespParser(responseText).Parse();
        }

        private async Task ConnectAsync(CancellationToken token)
        {
            if (_transport.State == TransportState.Closed)
            {
                await _transport.ConnectAsync(token);
                if (_settings.Password != null)
                {
                    var response = await ExecuteAsync(new[] { new KeyRequest(RequestType.Auth, _settings.Password) }, token);
                    if (!Equals(response[0].Value, "OK"))
                        throw new AuthenticationException("");
                }
            }
        }

        public class RespParser
        {
            private string _response;
            private int _index;

            public RespParser(string response)
            {
                _response = response;
            }

            public IResponse[] Parse()
            {
                var responses = new List<IResponse>();
                while (_response.Length > _index)
                {
                    responses.Add(ParseElement(_response));
                }

                return responses.ToArray();
            }

            private IResponse ParseElement(string response)
            {
                switch (response[_index++])
                {
                    case SimpleString:
                        return new StringResponse(ValueType.SimpleString, ParseSimpleString(response));
                    case Error:
                        return new StringResponse(ValueType.Error, ParseSimpleString(response));
                    case Integer:
                        return new IntegerResponse(ParseInteger(response));
                    case BulkString:
                        return new StringResponse(ValueType.BulkString, ParseBulkString(response));
                    case Array:
                        return new ArrayResponse(ParseArray(response));
                    default:
                        throw new ProtocolViolationException(Resources.ProtocolViolationInvalidBeginChar);
                }
            }

            private string ParseSimpleString(string response)
            {
                var builder = new StringBuilder(255);

                var c = response[_index++];
                while (c != CR && _index < response.Length)
                {
                    builder.Append(c);
                    c = response[_index++];
                }

                if (_index >= response.Length || response[_index++] != LF)
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

                return builder.ToString();
            }

            private int ParseInteger(string response)
            {
                var number = 0;
                var sign = 1;

                var c = response[_index];
                if (c == '-')
                {
                    sign = -1;
                    _index++;
                }

                c = response[_index++];
                while (_index < response.Length && c != CR)
                {
                    number = number * 10 + c - '0';
                    c = response[_index++];
                }

                if (_index >= response.Length || response[_index++] != LF)
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

                return number * sign;
            }

            private string ParseBulkString(string response)
            {
                var length = ParseInteger(response);
                if (length == -1)
                    return null;

                var value = response.Substring(_index, length);
                _index += length;

                if (_index >= response.Length || response[_index++] != CR || response[_index++] != LF)
                    throw new ProtocolViolationException(Resources.ProtocolViolationInvalidEndChar);

                return value;
            }

            private object[] ParseArray(string response)
            {
                var length = ParseInteger(response);
                if (length == -1)
                    return null;

                var array = new object[length];
                for (var arrayIndex = 0; arrayIndex < length; arrayIndex++)
                {
                    array[arrayIndex] = ParseElement(response).Value;
                }

                return array;
            }
        }
    }
}