using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenSearch.Net;
using Serilog.Formatting.OpenSearch;

namespace Serilog.Sinks.OpenSearch
{
    internal class SerializerAdapter : ISerializer, IOpenSearchSerializer
    {
        private readonly IOpenSearchSerializer _openSearchSerializer;

        internal SerializerAdapter(IOpenSearchSerializer openSearchSerializer)
        {
            _openSearchSerializer = openSearchSerializer ??
                                       throw new ArgumentNullException(nameof(openSearchSerializer));
        }

        public object Deserialize(Type type, Stream stream)
        {
            return _openSearchSerializer.Deserialize(type, stream);
        }

        public T Deserialize<T>(Stream stream)
        {
            return _openSearchSerializer.Deserialize<T>(stream);
        }

        public Task<object> DeserializeAsync(Type type, Stream stream,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            return _openSearchSerializer.DeserializeAsync(type, stream, cancellationToken);
        }

        public Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default(CancellationToken))
        {
            return _openSearchSerializer.DeserializeAsync<T>(stream, cancellationToken);
        }

        public void Serialize<T>(T data, Stream stream,
           SerializationFormatting formatting = SerializationFormatting.Indented)
        {
            _openSearchSerializer.Serialize(data, stream, formatting);
        }

        public Task SerializeAsync<T>(T data, Stream stream,
           SerializationFormatting formatting = SerializationFormatting.Indented,
           CancellationToken cancellationToken = default(CancellationToken))
        {
            return _openSearchSerializer.SerializeAsync(data, stream, formatting, cancellationToken);
        }

        public string SerializeToString(object value)
        {
            return _openSearchSerializer.SerializeToString(value, formatting: SerializationFormatting.None);
        }
    }
}