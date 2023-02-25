using System;
using System.Security.Cryptography.X509Certificates;
using Elastic.OpenSearch.Ephemeral;
using Elastic.OpenSearch.Xunit;
using Elastic.OpenSearch.Xunit.XunitPlumbing;
using OpenSearch.Net;
using Nest;
using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;
using Xunit;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch7.Bootstrap
{
	public abstract class OpenSearch7XTestBase : IClusterFixture<OpenSearch7XCluster> 
	{
        protected OpenSearch7XTestBase(OpenSearch7XCluster cluster) => Cluster = cluster;

        private OpenSearch7XCluster Cluster { get; }

        protected IElasticClient Client => this.CreateClient();

		protected virtual ConnectionSettings SetClusterSettings(ConnectionSettings s) => s;
        
        private IElasticClient CreateClient() =>
            Cluster.GetOrAddClient(c =>
            {
                var clusterNodes = c.NodesUris(ProxyDetection.LocalOrProxyHost);
                var settings = new ConnectionSettings(new StaticConnectionPool(clusterNodes));
                if (ProxyDetection.RunningMitmProxy)
                    settings = settings.Proxy(new Uri("http://localhost:8080"), null, (string)null);
                settings = SetClusterSettings(settings);

                var current = (IConnectionConfigurationValues)settings;
                var notAlreadyAuthenticated = current.BasicAuthenticationCredentials == null && current.ClientCertificates == null;
                var noCertValidation = current.ServerCertificateValidationCallback == null;

                var config = Cluster.ClusterConfiguration;
                if (config.EnableSecurity && notAlreadyAuthenticated)
                    settings = settings.BasicAuthentication(ClusterAuthentication.Admin.Username, ClusterAuthentication.Admin.Password);
                if (config.EnableSsl && noCertValidation)
                {
                    //todo use CA callback instead of allow all
                    // ReSharper disable once UnusedVariable
                    var ca = new X509Certificate2(config.FileSystem.CaCertificate);
                    settings = settings.ServerCertificateValidationCallback(CertificateValidations.AllowAll);
                }
                var client = new ElasticClient(settings);
                return client;
            });
    }

}
