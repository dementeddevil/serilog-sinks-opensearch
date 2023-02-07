#nullable enable
using System;
using OpenSearch.Net;
using Serilog.Debugging;

namespace Serilog.Sinks.OpenSearch.Sinks.OpenSearch
{
    /// <summary>
    /// Encapsulates detection of OpenSearch version
    /// and fallback in case of detection failure.
    /// </summary>
    internal class OpenSearchVersionManager
    {
        private readonly bool _detectOpenSearchVersion;
        private readonly IOpenSearchLowLevelClient _client;

        /// <summary>
        /// We are defaulting to version 1.0
        /// </summary>
        public readonly Version DefaultVersion = new(1, 0);
        public Version? DetectedVersion { get; private set; }
        public bool DetectionAttempted { get; private set; }

        public OpenSearchVersionManager(
            bool detectOpenSearchVersion,
            IOpenSearchLowLevelClient client)
        {
            _detectOpenSearchVersion = detectOpenSearchVersion;
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Version EffectiveVersion
        {
            get
            {
                if (DetectedVersion is not null)
                    return DetectedVersion;

                if (_detectOpenSearchVersion == false
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

                var version = new Version(strVersion);

                // Handle AWS OpenSearch running in compatibility mode
                var tagLine = response.Dictionary["tagline"];
                if (tagLine.HasValue &&
                    tagLine.Value.ToString().StartsWith("The OpenSearch Project", StringComparison.OrdinalIgnoreCase) &&
                    version.Major == 7)
                {
                    version = new Version(1, 0, 0);
                }

                return version;
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
