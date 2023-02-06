using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using Serilog.Formatting.Opensearch;

namespace Serilog.Sinks.Opensearch
{
    internal class SerializerAdapter : ISerializer, IOpenSearchSerializer
    {
        private readonly IOpenSearchSerializer _opensearchSerializer;

        internal SerializerAdapter(IOpenSearchSerializer opensearchSerializer)
        {
            _opensearchSerializer = opensearchSerializer ??
                                       throw new ArgumentNullException(nameof(opensearchSerializer));
        }

        public object Deserialize(Type type, Stream stream)
        {
            return _opensearchSerializer.Deserialize(type, stream);
        }

        public T Deserialize<T>(Stream stream)
        {
            return _opensearchSerializer.Deserialize<T>(stream);
        }

        public Task<object> DeserializeAsync(Type type, Stream stream,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            return _opensearchSerializer.DeserializeAsync(type, stream, cancellationToken);
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _opensearchSerializer.DeserializeAsync<T>(stream, cancellationToken);
        }

        public void Serialize<T>(T data, Stream stream,
           SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            _opensearchSerializer.Serialize(data, stream, formatting);
        }

        public Task SerializeAsync<T>(T data, Stream stream,
           SerializationFormatting formatting = SerializationFormatting.Indented,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            return _opensearchSerializer.SerializeAsync(data, stream, formatting, cancellationToken);
        }

        public string SerializeToString(object value)
        {
            return _opensearchSerializer.SerializeToString(value, formatting: SerializationFormatting.None);
        }
    }
}