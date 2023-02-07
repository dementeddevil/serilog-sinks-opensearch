﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpenSearch.Net;
using Serilog.Debugging;

namespace Serilog.Sinks.OpenSearch.Durable
{
    /// <summary>
    /// 
    /// </summary>
    public class OpenSearchLogClient : ILogClient<List<string>>
    {
        private readonly IOpenSearchLowLevelClient _openSearchLowLevelClient;
        private readonly Func<string, long?, string, string> _cleanPayload;
        private readonly OpenOpType _elasticOpType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="openSearchLowLevelClient"></param>
        /// <param name="cleanPayload"></param>
        /// <param name="elasticOpType"></param>
        public OpenSearchLogClient(IOpenSearchLowLevelClient openSearchLowLevelClient,
            Func<string, long?, string, string> cleanPayload,
            OpenOpType elasticOpType)
        {
            _openSearchLowLevelClient = openSearchLowLevelClient;
            _cleanPayload = cleanPayload;
            _elasticOpType = elasticOpType;
        }

        public async Task<SentPayloadResult> SendPayloadAsync(List<string> payload)
        {
            return await SendPayloadAsync(payload, true);
        }

        public async Task<SentPayloadResult> SendPayloadAsync(List<string> payload, bool first)
        {
            try
            {
                if (payload == null || !payload.Any()) return new SentPayloadResult(null, true);
                var response = await _openSearchLowLevelClient.BulkAsync<DynamicResponse>(PostData.MultiJson(payload));

                if (response.Success)
                {
                    var cleanPayload = new List<string>();
                    var invalidPayload = GetInvalidPayloadAsync(response, payload, out cleanPayload);
                    if ((cleanPayload?.Any() ?? false) && first)
                    {
                        await SendPayloadAsync(cleanPayload, false);
                    }

                    return new SentPayloadResult(response, true, invalidPayload);
                }
                else
                {
                    SelfLog.WriteLine("Received failed OpenSearch shipping result {0}: {1}", response.HttpStatusCode,
                        response.OriginalException);
                    return new SentPayloadResult(response, false,
                        new InvalidResult()
                        {
                            StatusCode = response.HttpStatusCode ?? 500,
                            Content = response.OriginalException.ToString()
                        });
                }
            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, ex);
                return new SentPayloadResult(null, false, null, ex);
            }


        }

        private InvalidResult GetInvalidPayloadAsync(DynamicResponse baseResult, List<string> payload, out List<string> cleanPayload)
        {
            int i = 0;
            cleanPayload = new List<string>();
            var items = baseResult.Body["items"];
            if (items == null) return null;
            List<string> badPayload = new List<string>();

            bool hasErrors = false;
            foreach (dynamic item in items)
            {
                var itemIndex = item?[BatchedOpenSearchSink.BulkAction(_elasticOpType)];
                long? status = itemIndex?["status"];
                i++;
                if (!status.HasValue || status < 300)
                {
                    continue;
                }

                hasErrors = true;
                var id = itemIndex?["_id"];
                var error = itemIndex?["error"];
                var errorString = $"type: {error?["type"] ?? "Unknown"}, reason: {error?["reason"] ?? "Unknown"}";

                if (int.TryParse(id.Split('_')[0], out int index))
                {
                    SelfLog.WriteLine("Received failed OpenSearch shipping result {0}: {1}. Failed payload : {2}.", status, errorString, payload.ElementAt(index * 2 + 1));
                    badPayload.Add(payload.ElementAt(index * 2));
                    badPayload.Add(payload.ElementAt(index * 2 + 1));
                    if (_cleanPayload != null)
                    {
                        cleanPayload.Add(payload.ElementAt(index * 2));
                        cleanPayload.Add(_cleanPayload(payload.ElementAt(index * 2 + 1), status, errorString));
                    }
                }
                else
                {
                    SelfLog.WriteLine($"Received failed OpenSearch shipping result {status}: {errorString}.");
                }
            }

            if (!hasErrors)
                return null;
            return new InvalidResult()
            {
                StatusCode = baseResult.HttpStatusCode ?? 500,
                Content = baseResult.ToString(),
                BadPayLoad = String.Join(Environment.NewLine, badPayload)
            };
        }
    }
}
