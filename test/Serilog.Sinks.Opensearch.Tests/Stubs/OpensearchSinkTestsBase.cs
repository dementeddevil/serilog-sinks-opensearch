using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using OpenSearch.Client;
using OpenSearch.Client.JsonNetSerializer;
using OpenSearch.Net;
using Serilog.Sinks.Opensearch.Tests.Domain;

namespace Serilog.Sinks.Opensearch.Tests.Stubs
{
    public abstract partial class OpensearchSinkTestsBase
    {
        static readonly TimeSpan TinyWait = TimeSpan.FromMilliseconds(50);
        protected readonly IConnection _connection;
        protected readonly OpensearchSinkOptions _options;
        protected List<string> _seenHttpPosts = new List<string>();
        protected List<int> _seenHttpHeads = new List<int>();
        protected List<Tuple<Uri, int>> _seenHttpGets = new List<Tuple<Uri, int>>();
        protected List<Tuple<Uri, string>> _seenHttpPuts = new List<Tuple<Uri, string>>();
        private IOpenSearchSerializer _serializer;

        protected int _templateExistsReturnCode = 404;

        protected OpensearchSinkTestsBase()
        {
            _seenHttpPosts = new List<string>();
            _seenHttpHeads = new List<int>();
            _seenHttpGets = new List<Tuple<Uri, int>>();
            _seenHttpPuts = new List<Tuple<Uri, string>>();

            var connectionPool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));
            _connection = new ConnectionStub(_seenHttpPosts,
                                             _seenHttpHeads,
                                             _seenHttpPuts,
                                             _seenHttpGets,
                                             () => _templateExistsReturnCode);
            _serializer = JsonNetSerializer.Default(LowLevelRequestResponseSerializer.Instance, new ConnectionSettings(connectionPool, _connection));

            _options = new OpensearchSinkOptions(connectionPool)
            {
                BatchPostingLimit = 2,
                //Period = TinyWait,
                Connection = _connection,
                Serializer = _serializer,
                PipelineName = "testPipe",
            };
        }

        /// <summary>
        /// Returns the posted serilog messages and validates the entire bulk in the process
        /// </summary>
        /// <param name="expectedCount"></param>
        /// <returns></returns>
        protected IList<SerilogOpensearchEvent> GetPostedLogEvents(int expectedCount)
        {
            _seenHttpPosts.Should().NotBeNullOrEmpty();
            var totalBulks = _seenHttpPosts.SelectMany(p => p.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)).ToList();
            totalBulks.Should().NotBeNullOrEmpty().And.HaveCount(expectedCount * 2);

            var bulkActions = new List<SerilogOpensearchEvent>();
            for (var i = 0; i < totalBulks.Count; i += 2)
            {
                BulkOperation action;
                try
                {
                    action = Deserialize<BulkOperation>(totalBulks[i]);
                }
                catch (Exception e)
                {
                    throw new Exception($"Can not deserialize into BulkOperation \r\n:{totalBulks[i]}", e);
                }
                action.IndexAction.Should().NotBeNull();
                action.IndexAction.Index.Should().NotBeNullOrEmpty().And.StartWith("logstash-");
                action.IndexAction.Type.Should().BeNull();

                SerilogOpensearchEvent actionMetaData;
                try
                {
                    actionMetaData = Deserialize<SerilogOpensearchEvent>(totalBulks[i + 1]);
                }
                catch (Exception e)
                {
                    throw new Exception(
                        $"Can not deserialize into SerilogOpensearchMessage \r\n:{totalBulks[i + 1]}", e);
                }
                actionMetaData.Should().NotBeNull();
                bulkActions.Add(actionMetaData);
            }
            return bulkActions;
        }

        protected T Deserialize<T>(string json)
        {
            return _serializer.Deserialize<T>(new MemoryStream(Encoding.UTF8.GetBytes(json)));
        }

        protected async Task ThrowAsync()
        {
            await Task.Delay(1);
            throw new Exception("boom!");
        }

        protected string[] AssertSeenHttpPosts(List<string> _seenHttpPosts, int lastN, int expectedNumberOfRequests = 2)
        {
            _seenHttpPosts.Should().NotBeEmpty().And.HaveCount(expectedNumberOfRequests);
            var json = string.Join("", _seenHttpPosts);
            var bulkJsonPieces = json.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            bulkJsonPieces.Count().Should().BeGreaterOrEqualTo(lastN);
            var skip = Math.Max(0, bulkJsonPieces.Count() - lastN);

            return bulkJsonPieces.Skip(skip).Take(lastN).ToArray();
        }

        protected void JsonEquals(string json, string embeddedResourceNameEndsWith)
        {
#if NETFRAMEWORK
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();       
#else
            var assembly = GetType().Assembly;
#endif
            var expected = TestDataHelper.ReadEmbeddedResource(assembly, embeddedResourceNameEndsWith);

            var nJson = JObject.Parse(json);
            var nOtherJson = JObject.Parse(expected);
            var equals = JToken.DeepEquals(nJson, nOtherJson);
            if (equals) return;
            expected.Should().BeEquivalentTo(json);
        }
    }
}