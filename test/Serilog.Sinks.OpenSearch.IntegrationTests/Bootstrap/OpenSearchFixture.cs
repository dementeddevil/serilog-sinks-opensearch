using System;
using System.Threading.Tasks;
using OpenSearch.Client;
using Testcontainers.OpenSearch;
using TUnit.Core.Interfaces;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap
{
    public abstract class OpenSearchFixture : IAsyncInitializer, IAsyncDisposable
    {
        private readonly OpenSearchContainer _container;

        protected OpenSearchFixture(string image, int expectedMajorVersion)
        {
            Image = image;
            ExpectedMajorVersion = expectedMajorVersion;
            _container = new OpenSearchBuilder(image)
                .WithSecurityEnabled(false)
                .WithEnvironment("OPENSEARCH_JAVA_OPTS", "-Xms512m -Xmx512m")
                .Build();
        }

        public string Image { get; }

        public int ExpectedMajorVersion { get; }

        public Uri Endpoint { get; private set; }

        public IOpenSearchClient Client { get; private set; }

        public async Task InitializeAsync()
        {
            await _container.StartAsync().ConfigureAwait(false);

            Endpoint = new Uri(_container.GetConnectionString());
            Client = new OpenSearchClient(new ConnectionSettings(Endpoint));

            var info = await Client.RootNodeInfoAsync().ConfigureAwait(false);
            if (!info.IsValid)
                throw new InvalidOperationException($"OpenSearch container '{Image}' did not return valid node information: {info.DebugInformation}");

            if (info.Version?.Number == null ||
                !Version.TryParse(info.Version.Number, out var version) ||
                version.Major != ExpectedMajorVersion)
            {
                throw new InvalidOperationException(
                    $"Expected OpenSearch major version {ExpectedMajorVersion}, but container '{Image}' reported '{info.Version?.Number ?? "unknown"}'.");
            }
        }

        public async ValueTask DisposeAsync()
        {
            await _container.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }

    public sealed class OpenSearch1Fixture : OpenSearchFixture
    {
        public OpenSearch1Fixture()
            : base("opensearchproject/opensearch:1.3.20", 1)
        {
        }
    }

    public sealed class OpenSearch2Fixture : OpenSearchFixture
    {
        public OpenSearch2Fixture()
            : base("opensearchproject/opensearch:2.19.6", 2)
        {
        }
    }
}
