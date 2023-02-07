using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using OpenSearch.Net;
using System.Threading;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal class ConnectionStub : InMemoryConnection
    {
        private readonly Func<int> _templateExistReturnCode;
        private readonly List<int> _seenHttpHeads;
        private readonly List<Tuple<Uri, int>> _seenHttpGets;
        private readonly List<string> _seenHttpPosts;
        private readonly List<Tuple<Uri, string>> _seenHttpPuts;

        public ConnectionStub(
            List<string> _seenHttpPosts,
            List<int> _seenHttpHeads,
            List<Tuple<Uri, string>> _seenHttpPuts,
            List<Tuple<Uri, int>> _seenHttpGets,
            Func<int> templateExistReturnCode
            ) : base()
        {
            this._seenHttpPosts = _seenHttpPosts;
            this._seenHttpHeads = _seenHttpHeads;
            this._seenHttpPuts = _seenHttpPuts;
            this._seenHttpGets = _seenHttpGets;
            _templateExistReturnCode = templateExistReturnCode;
        }

        public override TReturn Request<TReturn>(RequestData requestData)
        {
            byte[] responseBytes = Array.Empty<byte>();
            if (requestData.PostData != null)
            {
                using var ms = new MemoryStream();
                requestData.PostData.Write(ms, new ConnectionConfiguration());
                responseBytes = ms.ToArray();
            }

            int responseStatusCode = 200;
            string contentType = null;

            switch (requestData.Method)
            {
                case HttpMethod.PUT:
                    _seenHttpPuts.Add(Tuple.Create(requestData.Uri, Encoding.UTF8.GetString(responseBytes)));
                    break;
                case HttpMethod.POST:
                    _seenHttpPosts.Add(Encoding.UTF8.GetString(responseBytes));
                    break;
                case HttpMethod.GET:
                    _seenHttpGets.Add(Tuple.Create(requestData.Uri, responseStatusCode));
                    break;
                case HttpMethod.HEAD:
                    if (requestData.Uri.PathAndQuery.ToLower().StartsWith("/_template/"))
                    {
                        responseStatusCode = _templateExistReturnCode();
                    }
                    _seenHttpHeads.Add(responseStatusCode);
                    break;
            }

            return ReturnConnectionStatus<TReturn>(requestData, responseBytes, responseStatusCode, contentType);
        }

        public override Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
        {
            return Task.FromResult(Request<TResponse>(requestData));
        }
    }
}