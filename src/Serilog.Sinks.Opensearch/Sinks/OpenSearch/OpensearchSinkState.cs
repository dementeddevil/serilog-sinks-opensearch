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
using System.Text.RegularExpressions;
using OpenSearch.Net;
using OpenSearch.Net.Specification.IndicesApi;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.OpenSearch;
using Serilog.Sinks.OpenSearch.Sinks.OpenSearch;

namespace Serilog.Sinks.OpenSearch
{
    internal class OpenSearchSinkState
    {
        public static OpenSearchSinkState Create(OpenSearchSinkOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            return new OpenSearchSinkState(options);
        }

        private readonly OpenSearchSinkOptions _options;

        readonly Func<LogEvent, DateTimeOffset, string> _indexDecider;
        readonly Func<string, DateTime, string> _bufferedIndexDecider;

        private readonly ITextFormatter _formatter;
        private readonly ITextFormatter _durableFormatter;

        private readonly OpenSearchLowLevelClient _client;

        private readonly bool _registerTemplateOnStartup;
        private readonly string _templateName;
        private readonly string _templateMatchString;
        private static readonly Regex IndexFormatRegex = new Regex(@"^(.*)(?:\{0\:.+\})(.*)$");

        private readonly OpenSearchVersionManager _versionManager;

        private bool IncludeTypeName => true; //_versionManager.EffectiveVersion.Major >= 7;

        public OpenSearchSinkOptions Options => _options;
        public IOpenSearchLowLevelClient Client => _client;
        public ITextFormatter Formatter => _formatter;
        public ITextFormatter DurableFormatter => _durableFormatter;

        public bool TemplateRegistrationSuccess { get; private set; }

        private OpenSearchSinkState(OpenSearchSinkOptions options)
        {
            if (string.IsNullOrWhiteSpace(options.IndexFormat)) throw new ArgumentException("options.IndexFormat");
            if (string.IsNullOrWhiteSpace(options.TemplateName)) throw new ArgumentException("options.TemplateName");

            // Since TypeName is deprecated we shouldn't set it, if has been deliberately set to null.
            //if (options.TypeName != null && options.AutoRegisterTemplateVersion == AutoRegisterTemplateVersion.ESv7)
            //{
            //    options.TypeName = "_doc";
            //}

            _templateName = options.TemplateName;
            _templateMatchString = IndexFormatRegex.Replace(options.IndexFormat, @"$1*$2");

            _indexDecider = options.IndexDecider ?? ((@event, offset) => string.Format(options.IndexFormat, offset).ToLowerInvariant());
            _bufferedIndexDecider = options.BufferIndexDecider ?? ((@event, offset) => string.Format(options.IndexFormat, offset).ToLowerInvariant());

            _options = options;

            var configuration = new ConnectionConfiguration(options.ConnectionPool, options.Connection, options.Serializer)
                .RequestTimeout(options.ConnectionTimeout);

            if (options.ModifyConnectionSettings != null)
                configuration = options.ModifyConnectionSettings(configuration);

            configuration.ThrowExceptions();

            _client = new OpenSearchLowLevelClient(configuration);

            _formatter = options.CustomFormatter ?? CreateDefaultFormatter(options);

            _durableFormatter = options.CustomDurableFormatter ?? CreateDefaultDurableFormatter(options);

            _registerTemplateOnStartup = options.AutoRegisterTemplate;
            TemplateRegistrationSuccess = !_registerTemplateOnStartup;

            _versionManager = new OpenSearchVersionManager(options.DetectOpenSearchVersion, _client);

            // Resolve typeName (looks like this is removed in all opensearch versions)
            // Handle version detection now - we can discard the result...
            var version = _versionManager.EffectiveVersion;
            /*if (_versionManager.EffectiveVersion.Major < 7)
                _options.TypeName = string.IsNullOrWhiteSpace(_options.TypeName)
                    ? OpenSearchSinkOptions.DefaultTypeName // "logevent"
                    : _options.TypeName;
            else*/
                _options.TypeName = null;
        }

        public static ITextFormatter CreateDefaultFormatter(OpenSearchSinkOptions options)
        {
            return new OpenSearchJsonFormatter(
                formatProvider: options.FormatProvider,
                closingDelimiter: string.Empty,
                serializer: options.Serializer != null ? new SerializerAdapter(options.Serializer) : null,
                inlineFields: options.InlineFields,
                formatStackTraceAsArray: options.FormatStackTraceAsArray
            );
        }

        public static ITextFormatter CreateDefaultDurableFormatter(OpenSearchSinkOptions options)
        {
            return new OpenSearchJsonFormatter(
               formatProvider: options.FormatProvider,
               closingDelimiter: Environment.NewLine,
               serializer: options.Serializer != null ? new SerializerAdapter(options.Serializer) : null,
               inlineFields: options.InlineFields,
               formatStackTraceAsArray: options.FormatStackTraceAsArray
           );
        }

        public string Serialize(object o)
        {
            return _client.Serializer.SerializeToString(o, formatting: SerializationFormatting.None);
        }

        public string GetIndexForEvent(LogEvent e, DateTimeOffset offset)
        {
            if (!TemplateRegistrationSuccess && _options.RegisterTemplateFailure == RegisterTemplateRecovery.IndexToDeadletterIndex)
                return string.Format(_options.DeadLetterIndexName, offset);
            return _indexDecider(e, offset);
        }

        public string GetBufferedIndexForEvent(string logEvent, DateTime offset)
        {
            if (!TemplateRegistrationSuccess && _options.RegisterTemplateFailure == RegisterTemplateRecovery.IndexToDeadletterIndex)
                return string.Format(_options.DeadLetterIndexName, offset);
            return _bufferedIndexDecider(logEvent, offset);
        }

        /// <summary>
        /// Register the opensearch index template if the provided options mandate it.
        /// </summary>
        public void RegisterTemplateIfNeeded()
        {
            if (!_registerTemplateOnStartup) return;

            try
            {
                if (!_options.OverwriteTemplate)
                {
                    var templateExistsResponse = _client.Indices
                        .TemplateExistsForAll<VoidResponse>(
                            _templateName, 
                            new IndexTemplateExistsRequestParameters
                            {
                                RequestConfiguration = new RequestConfiguration() { AllowedStatusCodes = new[] { 200, 404 } }
                            });
                    if (templateExistsResponse.HttpStatusCode == 200)
                    {
                        TemplateRegistrationSuccess = true;
                        return;
                    }
                }

                // NOTE: OpenSearch from v1 onwards is compatible with Elasticsearch v8 templates
                // This is even true for AWS OpenSearch when running in ESv7 compatibility mode
                var result = _client.Indices.PutTemplateV2ForAll<StringResponse>(_templateName, GetTemplatePostData());
                if (!result.Success)
                {
                    ((IOpenSearchResponse)result).TryGetServerErrorReason(out var serverError);
                    SelfLog.WriteLine("Unable to create the template. {0}", serverError);

                    if (_options.RegisterTemplateFailure == RegisterTemplateRecovery.FailSink)
                        throw new Exception($"Unable to create the template named {_templateName}.", result.OriginalException);

                    TemplateRegistrationSuccess = false;
                }
                else
                    TemplateRegistrationSuccess = true;
            }
            catch (Exception ex)
            {
                TemplateRegistrationSuccess = false;

                SelfLog.WriteLine("Failed to create the template. {0}", ex);

                if (_options.RegisterTemplateFailure == RegisterTemplateRecovery.FailSink)
                    throw;
            }
        }

        private PostData GetTemplatePostData()
        {
            //PostData no longer exposes an implicit cast from object.  Previously it supported that and would inspect the object Type to
            //determine if it it was a literal string to write directly or if it was an object that it needed to serialize.  Now the onus is 
            //on us to tell it what type we are passing otherwise if the user specified the template as a json string it would be serialized again.
            var template = GetTemplateData();
            if (template is string s)
            {
                return PostData.String(s);
            }
            else
            {
                return PostData.Serializable(template);
            }
        }

        private object GetTemplateData()
        {
            if (_options.GetTemplateContent != null)
                return _options.GetTemplateContent();

            var settings = _options.TemplateCustomSettings ?? new Dictionary<string, string>();

            if (!settings.ContainsKey("index.refresh_interval"))
                settings.Add("index.refresh_interval", "5s");

            if (_options.NumberOfShards.HasValue && !settings.ContainsKey("number_of_shards"))
                settings.Add("number_of_shards", _options.NumberOfShards.Value.ToString());

            if (_options.NumberOfReplicas.HasValue && !settings.ContainsKey("number_of_replicas"))
                settings.Add("number_of_replicas", _options.NumberOfReplicas.Value.ToString());

            var effectiveTemplateVersion =
                _options.AutoRegisterTemplateVersion ??
                _versionManager.EffectiveVersion.Major switch
                {
                    >= 1 => AutoRegisterTemplateVersion.OSv1,
                    _ => throw new NotSupportedException()
                };

            return OpenSearchTemplateProvider.GetTemplate(
                _options,
                _versionManager.EffectiveVersion.Major,
                settings,
                _templateMatchString,
                effectiveTemplateVersion);
        }
    }
}
