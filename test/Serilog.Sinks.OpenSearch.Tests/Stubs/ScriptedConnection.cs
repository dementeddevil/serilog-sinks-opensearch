using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;

namespace Serilog.Sinks.OpenSearch.Tests.Stubs
{
    internal sealed class ScriptedConnection : InMemoryConnection
    {
        private readonly ConcurrentQueue<Func<RequestData, object>> _steps = new();
        private readonly SemaphoreSlim _requests = new(0);

        public ConcurrentQueue<string> RequestBodies { get; } = new();

        public void EnqueueResponse(int statusCode = 200, string body = "{\"errors\":false}")
            => _steps.Enqueue(request => ReturnConnectionStatus<DynamicResponse>(request, Encoding.UTF8.GetBytes(body), statusCode, "application/json"));

        public void EnqueueException(Exception exception) => _steps.Enqueue(_ => throw exception);

        public async Task WaitForRequestAsync(TimeSpan timeout)
        {
            if (!await _requests.WaitAsync(timeout).ConfigureAwait(false))
                throw new TimeoutException("The expected OpenSearch request was not observed.");
        }

        public override TResponse Request<TResponse>(RequestData requestData)
        {
            Capture(requestData);
            if (!_steps.TryDequeue(out var step))
                return ReturnConnectionStatus<TResponse>(requestData, Encoding.UTF8.GetBytes("{\"errors\":false}"), 200, "application/json");

            var result = step(requestData);
            if (result is TResponse response) return response;
            return ReturnConnectionStatus<TResponse>(requestData, Encoding.UTF8.GetBytes("{\"errors\":false}"), 200, "application/json");
        }

        public override Task<TResponse> RequestAsync<TResponse>(RequestData requestData, CancellationToken cancellationToken)
            => Task.FromResult(Request<TResponse>(requestData));

        private void Capture(RequestData requestData)
        {
            if (requestData.PostData != null)
            {
                using var stream = new MemoryStream();
                requestData.PostData.Write(stream, new ConnectionConfiguration());
                RequestBodies.Enqueue(Encoding.UTF8.GetString(stream.ToArray()));
            }
            _requests.Release();
        }
    }
}
