using System;
using OpenSearch.Net;

namespace Serilog.Sinks.OpenSearch.IntegrationTests.Bootstrap
{
    public static class OpenSearchSinkOptionsFactory
    {
        public static OpenSearchSinkOptions Create(
            Uri endpoint,
            string indexPrefix,
            string templateName,
            Action<OpenSearchSinkOptions> alterOptions = null)
        {
            var options = new OpenSearchSinkOptions(new SingleNodeConnectionPool(endpoint))
            {
                IndexFormat = indexPrefix + "-{0:yyyy.MM.dd}",
                TemplateName = templateName,
                AutoRegisterTemplate = true,
                NumberOfReplicas = 0,
                BatchPostingLimit = 100,
                Period = TimeSpan.FromMilliseconds(100),
            };

            alterOptions?.Invoke(options);
            return options;
        }
    }
}
