#nullable enable
using System;
using OpenSearch.Net;
using Serilog.Debugging;

namespace Serilog.Sinks.Opensearch.Sinks.Opensearch
{
    /// <summary>
    /// Encapsulates detection of Opensearch version
    /// and fallback in case of detection failiure.
    /// </summary>
    internal class OpensearchVersionManager
    {
        private readonly bool _detectOpensearchVersion;
        private readonly IOpenSearchLowLevelClient _client;

        /// <summary>
        /// We are defaulting to version 1.0
        /// </summary>
        public readonly Version DefaultVersion = new(1, 0);
        public Version? DetectedVersion { get; private set; }
        public bool DetectionAttempted { get; private set; }

        public OpensearchVersionManager(
            bool detectOpensearchVersion,
            IOpenSearchLowLevelClient client)
        {
            _detectOpensearchVersion = detectOpensearchVersion;
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Version EffectiveVersion
        {
            get
            {
                if (DetectedVersion is not null)
                    return DetectedVersion;

                if (_detectOpensearchVersion == false
                    || DetectionAttempted == true)
                    return DefaultVersion;

                // Attempt once
                DetectedVersion = DiscoverClusterVersion();

                return DetectedVersion ?? DefaultVersion;
            }
        }

        internal Version? DiscoverClusterVersion()
        {
            try
            {
                var response = _client.DoRequest<DynamicResponse>(HttpMethod.GET, "/");
                if (!response.Success) return null;

                var discoveredVersion = response.Dictionary["version"]["number"];

                if (!discoveredVersion.HasValue)
                    return null;

                if (discoveredVersion.Value is not string strVersion)
                    return null;

                return new Version(strVersion);

            }
            catch (Exception ex)
            {
                SelfLog.WriteLine("Failed to discover the cluster version. {0}", ex);
                return null;
            }
            finally
            {
                DetectionAttempted = true;
            }
        }
    }
}
