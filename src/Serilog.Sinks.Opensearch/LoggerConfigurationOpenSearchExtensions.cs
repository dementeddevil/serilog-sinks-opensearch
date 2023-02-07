// Copyright 2014 Serilog Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.OpenSearch;
using System.Collections.Specialized;
using System.ComponentModel;
using OpenSearch.Net;
using Serilog.Formatting;
using Serilog.Sinks.OpenSearch.Durable;

namespace Serilog
{
    /// <summary>
    /// Adds the WriteTo.OpenSearch() extension method to <see cref="LoggerConfiguration"/>.
    /// </summary>
    public static class LoggerConfigurationOpenSearchExtensions
    {
        const string DefaultNodeUri = "http://localhost:9200";

        /// <summary>
        /// Adds a sink that writes log events as documents to an OpenSearch index.
        /// This works great with the Kibana web interface when using the default settings.
        /// 
        /// By passing in the BufferBaseFilename, you make this into a durable sink. 
        /// Meaning it will log to disk first and tries to deliver to the OpenSearch server in the background.
        /// </summary>
        /// <remarks>
        /// Make sure to have a sensible mapping in your OpenSearch indexes. 
        /// You can automatically create one by specifying this in the options.
        /// </remarks>
        /// <param name="loggerSinkConfiguration">Options for the sink.</param>
        /// <param name="options">Provides options specific to the OpenSearch sink</param>
        /// <returns>LoggerConfiguration object</returns>
        public static LoggerConfiguration OpenSearch(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            OpenSearchSinkOptions options = null)
        {
            options = options ?? new OpenSearchSinkOptions(new[] { new Uri(DefaultNodeUri) });

            var sink = string.IsNullOrWhiteSpace(options.BufferBaseFilename)
                ? (ILogEventSink)new OpenSearchSink(options)
                : new DurableOpenSearchSink(options);

            return loggerSinkConfiguration.Sink(
                sink,
                restrictedToMinimumLevel: options.MinimumLogEventLevel ?? LevelAlias.Minimum,
                levelSwitch: options.LevelSwitch
            );
        }

        /// <summary>
        /// Overload to allow basic configuration through AppSettings.
        /// </summary>
        /// <param name="loggerSinkConfiguration">Options for the sink.</param>
        /// <param name="nodeUris">A comma or semi-colon separated list of URIs for OpenSearch nodes.</param>
        /// <param name="indexFormat"><see cref="OpenSearchSinkOptions.IndexFormat"/></param>
        /// <param name="templateName"><see cref="OpenSearchSinkOptions.TemplateName"/></param>
        /// <param name="typeName"><see cref="OpenSearchSinkOptions.TypeName"/></param>
        /// <param name="batchPostingLimit"><see cref="OpenSearchSinkOptions.BatchPostingLimit"/></param>
        /// <param name="period"><see cref="OpenSearchSinkOptions.Period"/></param>
        /// <param name="inlineFields"><see cref="OpenSearchSinkOptions.InlineFields"/></param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <param name="bufferBaseFilename"><see cref="OpenSearchSinkOptions.BufferBaseFilename"/></param>
        /// <param name="bufferFileSizeLimitBytes"><see cref="OpenSearchSinkOptions.BufferFileSizeLimitBytes"/></param>
        /// <param name="bufferLogShippingInterval"><see cref="OpenSearchSinkOptions.BufferLogShippingInterval"/></param>
        /// <param name="connectionGlobalHeaders">A comma or semi-colon separated list of key value pairs of headers to be added to each elastic http request</param>
        [Obsolete("New code should not be compiled against this obsolete overload"), EditorBrowsable(EditorBrowsableState.Never)]
        public static LoggerConfiguration OpenSearch(
           this LoggerSinkConfiguration loggerSinkConfiguration,
           string nodeUris,
           string indexFormat,
           string templateName,
           string typeName,
           int batchPostingLimit,
           int period,
           bool inlineFields,
           LogEventLevel restrictedToMinimumLevel,
           string bufferBaseFilename,
           long? bufferFileSizeLimitBytes,
           long bufferLogShippingInterval,
           string connectionGlobalHeaders,
           LoggingLevelSwitch levelSwitch)
        {
            return OpenSearch(loggerSinkConfiguration, nodeUris, indexFormat, templateName, typeName, batchPostingLimit, period, inlineFields, restrictedToMinimumLevel, bufferBaseFilename,
                bufferFileSizeLimitBytes, bufferLogShippingInterval, connectionGlobalHeaders, levelSwitch, 5, EmitEventFailureHandling.WriteToSelfLog, 100000, null, false,
                AutoRegisterTemplateVersion.OSv1, false, RegisterTemplateRecovery.IndexAnyway, null, null, null);
        }

        /// <summary>
        /// Overload to allow basic configuration through AppSettings.
        /// </summary>
        /// <param name="loggerSinkConfiguration">Options for the sink.</param>
        /// <param name="nodeUris">A comma or semi-colon separated list of URIs for OpenSearch nodes.</param>
        /// <param name="indexFormat"><see cref="OpenSearchSinkOptions.IndexFormat"/></param>
        /// <param name="templateName"><see cref="OpenSearchSinkOptions.TemplateName"/></param>
        /// <param name="typeName"><see cref="OpenSearchSinkOptions.TypeName"/></param>
        /// <param name="batchPostingLimit"><see cref="OpenSearchSinkOptions.BatchPostingLimit"/></param>
        /// <param name="period"><see cref="OpenSearchSinkOptions.Period"/></param>
        /// <param name="inlineFields"><see cref="OpenSearchSinkOptions.InlineFields"/></param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink. Ignored when <paramref name="levelSwitch"/> is specified.</param>
        /// <param name="levelSwitch">A switch allowing the pass-through minimum level to be changed at runtime.</param>
        /// <param name="bufferBaseFilename"><see cref="OpenSearchSinkOptions.BufferBaseFilename"/></param>
        /// <param name="bufferFileSizeLimitBytes"><see cref="OpenSearchSinkOptions.BufferFileSizeLimitBytes"/></param>
        /// <param name="bufferFileCountLimit"><see cref="OpenSearchSinkOptions.BufferFileCountLimit"/></param>        
        /// <param name="bufferLogShippingInterval"><see cref="OpenSearchSinkOptions.BufferLogShippingInterval"/></param>
        /// <param name="connectionGlobalHeaders">A comma or semi-colon separated list of key value pairs of headers to be added to each elastic http request</param>
        /// <param name="connectionTimeout"><see cref="OpenSearchSinkOptions.ConnectionTimeout"/>The connection timeout (in seconds) when sending bulk operations to opensearch (defaults to 5).</param>   
        /// <param name="emitEventFailure"><see cref="OpenSearchSinkOptions.EmitEventFailure"/>Specifies how failing emits should be handled.</param>  
        /// <param name="queueSizeLimit"><see cref="OpenSearchSinkOptions.QueueSizeLimit"/>The maximum number of events that will be held in-memory while waiting to ship them to OpenSearch. Beyond this limit, events will be dropped. The default is 100,000. Has no effect on durable log shipping.</param>   
        /// <param name="pipelineName"><see cref="OpenSearchSinkOptions.PipelineName"/>Name the Pipeline where log events are sent to sink. Please note that the Pipeline should be existing before the usage starts.</param>   
        /// <param name="autoRegisterTemplate"><see cref="OpenSearchSinkOptions.AutoRegisterTemplate"/>When set to true the sink will register an index template for the logs in opensearch.</param>   
        /// <param name="autoRegisterTemplateVersion"><see cref="OpenSearchSinkOptions.AutoRegisterTemplateVersion"/>When using the AutoRegisterTemplate feature, this allows to set the OpenSearch version. Depending on the version, a template will be selected. Defaults to pre 5.0.</param>  
        /// <param name="overwriteTemplate"><see cref="OpenSearchSinkOptions.OverwriteTemplate"/>When using the AutoRegisterTemplate feature, this allows you to overwrite the template in OpenSearch if it already exists. Defaults to false</param>   
        /// <param name="registerTemplateFailure"><see cref="OpenSearchSinkOptions.RegisterTemplateFailure"/>Specifies the option on how to handle failures when writing the template to OpenSearch. This is only applicable when using the AutoRegisterTemplate option.</param>  
        /// <param name="deadLetterIndexName"><see cref="OpenSearchSinkOptions.DeadLetterIndexName"/>Optionally set this value to the name of the index that should be used when the template cannot be written to ES.</param>  
        /// <param name="numberOfShards"><see cref="OpenSearchSinkOptions.NumberOfShards"/>The default number of shards.</param>   
        /// <param name="numberOfReplicas"><see cref="OpenSearchSinkOptions.NumberOfReplicas"/>The default number of replicas.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="connection">Allows you to override the connection used to communicate with opensearch.</param>
        /// <param name="serializer">When passing a serializer unknown object will be serialized to object instead of relying on their ToString representation</param>
        /// <param name="connectionPool">The connectionpool describing the cluster to write event to</param>
        /// <param name="customFormatter">Customizes the formatter used when converting log events into OpenSearch documents. Please note that the formatter output must be valid JSON :)</param>
        /// <param name="customDurableFormatter">Customizes the formatter used when converting log events into the durable sink. Please note that the formatter output must be valid JSON :)</param>
        /// <param name="failureSink">Sink to use when OpenSearch is unable to accept the events. This is optionally and depends on the EmitEventFailure setting.</param>   
        /// <param name="singleEventSizePostingLimit"><see cref="OpenSearchSinkOptions.SingleEventSizePostingLimit"/>The maximum length of an event allowed to be posted to OpenSearch.default null</param>
        /// <param name="templateCustomSettings">Add custom opensearch settings to the template</param>
        /// <param name="detectOpenSearchVersion">Turns on detection of opensearch version via background HTTP call. This will also set `TypeName` automatically, according to the version of OpenSearch.</param>
        /// <param name="batchAction">Configures the OpType being used when inserting document in batch. Must be set to create for data streams.</param>
        /// <returns>LoggerConfiguration object</returns>
        /// <exception cref="ArgumentNullException"><paramref name="nodeUris"/> is <see langword="null" />.</exception>
        public static LoggerConfiguration OpenSearch(
            this LoggerSinkConfiguration loggerSinkConfiguration,
            string nodeUris,
            string indexFormat = null,
            string templateName = null,
            string typeName = null,
            int batchPostingLimit = 50,
            int period = 2,
            bool inlineFields = false,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            string bufferBaseFilename = null,
            long? bufferFileSizeLimitBytes = null,
            long bufferLogShippingInterval = 5000,
            string connectionGlobalHeaders = null,
            LoggingLevelSwitch levelSwitch = null,
            int connectionTimeout = 5,
            EmitEventFailureHandling emitEventFailure = EmitEventFailureHandling.WriteToSelfLog,
            int queueSizeLimit = 100000,
            string pipelineName = null,
            bool autoRegisterTemplate = false,
            AutoRegisterTemplateVersion? autoRegisterTemplateVersion = null,
            bool overwriteTemplate = false,
            RegisterTemplateRecovery registerTemplateFailure = RegisterTemplateRecovery.IndexAnyway,
            string deadLetterIndexName = null,
            int? numberOfShards = null,
            int? numberOfReplicas = null,
            IFormatProvider formatProvider = null,
            IConnection connection = null,
            IOpenSearchSerializer serializer = null,
            IConnectionPool connectionPool = null,
            ITextFormatter customFormatter = null,
            ITextFormatter customDurableFormatter = null,
            ILogEventSink failureSink = null,
            long? singleEventSizePostingLimit = null,
            int? bufferFileCountLimit = null,
            Dictionary<string,string> templateCustomSettings = null,
            OpenOpType batchAction = OpenOpType.Index,
            bool detectOpenSearchVersion = true)
        {
            if (string.IsNullOrEmpty(nodeUris))
                throw new ArgumentNullException(nameof(nodeUris), "No OpenSearch node(s) specified.");

            IEnumerable<Uri> nodes = nodeUris
                .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(uriString => new Uri(uriString));

            var options = connectionPool == null ? new OpenSearchSinkOptions(nodes) : new OpenSearchSinkOptions(connectionPool);

            if (!string.IsNullOrWhiteSpace(indexFormat))
            {
                options.IndexFormat = indexFormat;
            }

            if (!string.IsNullOrWhiteSpace(templateName))
            {
                options.AutoRegisterTemplate = true;
                options.TemplateName = templateName;
            }

            options.BatchPostingLimit = batchPostingLimit;
            options.BatchAction = batchAction;
            options.SingleEventSizePostingLimit = singleEventSizePostingLimit;
            options.Period = TimeSpan.FromSeconds(period);
            options.InlineFields = inlineFields;
            options.MinimumLogEventLevel = restrictedToMinimumLevel;
            options.LevelSwitch = levelSwitch;

            if (!string.IsNullOrWhiteSpace(bufferBaseFilename))
            {
                Path.GetFullPath(bufferBaseFilename);       // validate path
                options.BufferBaseFilename = bufferBaseFilename;
            }

            if (bufferFileSizeLimitBytes.HasValue)
            {
                options.BufferFileSizeLimitBytes = bufferFileSizeLimitBytes.Value;
            }

            if (bufferFileCountLimit.HasValue)
            {
                options.BufferFileCountLimit = bufferFileCountLimit.Value;
            }
            options.BufferLogShippingInterval = TimeSpan.FromMilliseconds(bufferLogShippingInterval);

            if (!string.IsNullOrWhiteSpace(connectionGlobalHeaders))
            {
                NameValueCollection headers = new NameValueCollection();
                connectionGlobalHeaders
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList()
                    .ForEach(headerValueStr =>
                    {
                        var headerValue = headerValueStr.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                        headers.Add(headerValue[0], headerValue[1]);
                    });

                options.ModifyConnectionSettings = (c) => c.GlobalHeaders(headers);
            }

            options.ConnectionTimeout = TimeSpan.FromSeconds(connectionTimeout);
            options.EmitEventFailure = emitEventFailure;
            options.QueueSizeLimit = queueSizeLimit;
            options.PipelineName = pipelineName;

            options.AutoRegisterTemplate = autoRegisterTemplate;
            options.AutoRegisterTemplateVersion = autoRegisterTemplateVersion;
            options.RegisterTemplateFailure = registerTemplateFailure;
            options.OverwriteTemplate = overwriteTemplate;
            options.NumberOfShards = numberOfShards;
            options.NumberOfReplicas = numberOfReplicas;

            if (!string.IsNullOrWhiteSpace(deadLetterIndexName))
            {
                options.DeadLetterIndexName = deadLetterIndexName;
            }

            options.FormatProvider = formatProvider;
            options.FailureSink = failureSink;
            options.Connection = connection;
            options.CustomFormatter = customFormatter;
            options.CustomDurableFormatter = customDurableFormatter;
            options.Serializer = serializer;

            options.TemplateCustomSettings = templateCustomSettings;

            options.DetectOpenSearchVersion = detectOpenSearchVersion;

            return OpenSearch(loggerSinkConfiguration, options);
        }
    }
}
