using Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.OpenSearch6.Bootstrap
{
	public class OpenSearch6XCluster : ClientTestClusterBase
	{
		public OpenSearch6XCluster() : base(CreateConfiguration()) { }

		private static ClientTestClusterConfiguration CreateConfiguration()
		{
			return new ClientTestClusterConfiguration("6.6.0")
			{
				MaxConcurrency = 1
			};
		}

		protected override void SeedCluster() { }
	}
}
