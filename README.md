# Serilog.Sinks.OpenSearch [![Continuous Integration](https://github.com/dementeddevil/serilog-sinks-opensearch/actions/workflows/cicd.yaml/badge.svg?branch=dev)](https://github.com/serilog-contrib/serilog-sinks-opensearch/actions/workflows/cicd.yaml) [![NuGet Badge](https://img.shields.io/nuget/v/Serilog.Sinks.OpenSearch.svg)](https://www.nuget.org/packages/Serilog.Sinks.OpenSearch)

This repository contains two nuget packages: `Serilog.Sinks.OpenSearch` and `Serilog.Formatting.OpenSearch`.

## Table of contents

* [What is this sink](#what-is-this-sink)
* [Features](#features)
* [Quick start](#quick-start)
  * [OpenSearch sinks](#opensearch-sinks)
  * [OpenSearch formatters](#opensearch-formatters)
* [More information](#more-information)
  * [A note about fields inside OpenSearch](#a-note-about-fields-inside-opensearch)
  * [A note about Kibana](#a-note-about-kibana)
  * [JSON `appsettings.json` configuration](#json-appsettingsjson-configuration)
  * [Handling errors](#handling-errors)
  * [Breaking changes](#breaking-changes)

## What is this sink

The Serilog OpenSearch sink project is a sink (basically a writer) for the Serilog logging framework. Structured log events are written to sinks and each sink is responsible for writing it to its own backend, database, store etc. This sink delivers the data to OpenSearch, a NoSQL search engine. It does this in a similar structure as Logstash and makes it easy to use Kibana for visualizing your logs.

## Features

* Simple configuration to get log events published to OpenSearch. Only server address is needed.
* All properties are stored inside fields in OpenSearch. This allows you to query on all the relevant data but also run analytics over this data.
* Be able to customize the store; specify the index name being used, the serializer or the connections to the server (load balanced).
* Durable mode; store the logevents first on disk before delivering them to OS making sure you never miss events if you have trouble connecting to your OS cluster.
* Automatically create the right mappings for the best usage of the log events in OS or automatically upload your own custom mapping.
* Versions 1 and 2 of OpenSearch supported. Version 1.0.0 of the sink targets netstandard2.0 and therefore can be run on any .NET Framework that supports it (both .NET Core and .NET Framework), however, we are focused on testing it with .NET 6.0/.NET 7.0 to make the maintenance simpler.


## Quick start

### OpenSearch sinks

```powershell
Install-Package serilog.sinks.opensearch
```

Simplest way to register this sink is to use default configuration:

```csharp
var loggerConfig = new LoggerConfiguration()
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200")));
```

Or, if using .NET Core and `Serilog.Settings.Configuration` Nuget package and `appsettings.json`, default configuration would look like this:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.OpenSearch" ],
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "OpenSearch",
        "Args": {
          "nodeUris": "http://localhost:9200"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName" ],
    "Properties": {
      "Application": "ImmoValuation.Swv - Web"
    }
  }
}
```

More elaborate configuration, using additional Nuget packages (e.g. `Serilog.Enrichers.Environment`) would look like:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.OpenSearch" ],
    "MinimumLevel": "Warning",
    "WriteTo": [
      {
        "Name": "OpenSearch",
        "Args": {
          "nodeUris": "http://localhost:9200"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName" ],
    "Properties": {
      "Application": "My app"
    }
  }
}
```

This way the sink will detect version of OpenSearch server (`DetectOpenSearchVersion` is set to `true` by default) and it will successfully handle AWS OpenSearch when it is running in Elasticsearch compatibility mode.

### Disable detection of OpenSearch server version

Alternatively, `DetectOpenSearchVersion` can be set to `false` and certain option can be configured manually. In that case, the sink will assume version 1 of OpenSearch, but options will be ignored due to a potential version incompatibility.

For example, you can configure the sink to force registration of v1 index template. Be aware that the AutoRegisterTemplate option will not overwrite an existing template.

```csharp
var loggerConfig = new LoggerConfiguration()
    .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200") ){
             DetectOpenSearchVersion = false,
             AutoRegisterTemplate = true,
             AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1
     });
```

### Configurable properties

Besides a registration of the sink in the code, it is possible to register it using appSettings reader (from v2.0.42+) reader (from v2.0.42+) as shown below.

This example shows the options that are currently available when using the appSettings reader.

```xml
  <appSettings>
    <add key="serilog:using" value="Serilog.Sinks.OpenSearch"/>
    <add key="serilog:write-to:OpenSearch.nodeUris" value="http://localhost:9200;http://remotehost:9200"/>
    <add key="serilog:write-to:OpenSearch.indexFormat" value="custom-index-{0:yyyy.MM}"/>
    <add key="serilog:write-to:OpenSearch.templateName" value="myCustomTemplate"/>
    <add key="serilog:write-to:OpenSearch.typeName" value="myCustomLogEventType"/>
    <add key="serilog:write-to:OpenSearch.pipelineName" value="myCustomPipelineName"/>
    <add key="serilog:write-to:OpenSearch.batchPostingLimit" value="50"/>
    <add key="serilog:write-to:OpenSearch.batchAction" value="Create"/><!-- "Index" is default -->
    <add key="serilog:write-to:OpenSearch.period" value="2"/>
    <add key="serilog:write-to:OpenSearch.inlineFields" value="true"/>
    <add key="serilog:write-to:OpenSearch.restrictedToMinimumLevel" value="Warning"/>
    <add key="serilog:write-to:OpenSearch.bufferBaseFilename" value="C:\Temp\SerilogOpenSearchBuffer"/>
    <add key="serilog:write-to:OpenSearch.bufferFileSizeLimitBytes" value="5242880"/>
    <add key="serilog:write-to:OpenSearch.bufferLogShippingInterval" value="5000"/>
    <add key="serilog:write-to:OpenSearch.bufferRetainedInvalidPayloadsLimitBytes" value="5000"/>
    <add key="serilog:write-to:OpenSearch.bufferFileCountLimit " value="31"/>
    <add key="serilog:write-to:OpenSearch.connectionGlobalHeaders" value="Authorization=Bearer SOME-TOKEN;OtherHeader=OTHER-HEADER-VALUE" />
    <add key="serilog:write-to:OpenSearch.connectionTimeout" value="5" />
    <add key="serilog:write-to:OpenSearch.emitEventFailure" value="WriteToSelfLog" />
    <add key="serilog:write-to:OpenSearch.queueSizeLimit" value="100000" />
    <add key="serilog:write-to:OpenSearch.autoRegisterTemplate" value="true" />
    <add key="serilog:write-to:OpenSearch.autoRegisterTemplateVersion" value="OSv1" />
    <add key="serilog:write-to:OpenSearch.detectOpenSearchVersion" value="false" /><!-- `true` is default -->
    <add key="serilog:write-to:OpenSearch.overwriteTemplate" value="false" />
    <add key="serilog:write-to:OpenSearch.registerTemplateFailure" value="IndexAnyway" />
    <add key="serilog:write-to:OpenSearch.deadLetterIndexName" value="deadletter-{0:yyyy.MM}" />
    <add key="serilog:write-to:OpenSearch.numberOfShards" value="20" />
    <add key="serilog:write-to:OpenSearch.numberOfReplicas" value="10" />
    <add key="serilog:write-to:OpenSearch.formatProvider" value="My.Namespace.MyFormatProvider, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.connection" value="My.Namespace.MyConnection, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.serializer" value="My.Namespace.MySerializer, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.connectionPool" value="My.Namespace.MyConnectionPool, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.customFormatter" value="My.Namespace.MyCustomFormatter, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.customDurableFormatter" value="My.Namespace.MyCustomDurableFormatter, My.Assembly.Name" />
    <add key="serilog:write-to:OpenSearch.failureSink" value="My.Namespace.MyFailureSink, My.Assembly.Name" />
  </appSettings>
```

With the appSettings configuration the `nodeUris` property is required. Multiple nodes can be specified using `,` or `;` to separate them. All other properties are optional. Also required is the `<add key="serilog:using" value="Serilog.Sinks.OpenSearch"/>` setting to include this sink. All other properties are optional. If you do not explicitly specify an indexFormat-setting, a generic index such as 'logstash-[current_date]' will be used automatically.

And start writing your events using Serilog.

### OpenSearch formatters

```powershell
Install-Package serilog.formatting.opensearch
```

The `Serilog.Formatting.OpenSearch` nuget package consists of a several formatters:

* `OpenSearchJsonFormatter` - custom json formatter that respects the configured property name handling and forces `Timestamp` to @timestamp.
* `ExceptionAsObjectJsonFormatter` - a json formatter which serializes any exception into an exception object.

Override default formatter if it's possible with selected sink

```csharp
var loggerConfig = new LoggerConfiguration()
  .WriteTo.Console(new OpenSearchJsonFormatter());
```

## More information

* [Basic information](https://github.com/dementeddevil/serilog-sinks-opensearch/wiki/basic-setup) on how to configure and use this sink.
* [Configuration options](https://github.com/dementeddevil/serilog-sinks-opensearch/wiki/Configure-the-sink) which you can use.
* How to use the [durability](https://github.com/dementeddevil/serilog-sinks-opensearch/wiki/durability) mode.
* Get the [NuGet package](http://www.nuget.org/packages/Serilog.Sinks.OpenSearch).
* Report issues to the [issue tracker](https://github.com/dementeddevil/serilog-sinks-opensearch/issues). PR welcome, but please do this against the dev branch.
* For an overview of recent changes, have a look at the [change log](https://github.com/dementeddevil/serilog-sinks-opensearch/blob/master/CHANGES.md).

### A note about fields inside OpenSearch

Be aware that there is an explicit and implicit mapping of types inside an OpenSearch index. A value called `X` as a string will be indexed as being a string. Sending the same `X` as an integer in a next log message will not work. OpenSearch will raise a mapping exception, however it is not that evident that your log item was not stored due to the bulk actions performed.

So be careful about defining and using your fields (and type of fields). It is easy to miss that you first send a {User} as a simple username (string) and next as a User object. The first mapping dynamically created in the index wins. There are also limits in OpenSearch on the number of dynamic fields you can actually throw inside an index.

### A note about Kibana

In order to avoid a potentially deeply nested JSON structure for exceptions with inner exceptions,
by default the logged exception and it's inner exception is logged as an array of exceptions in the field `exceptions`. Use the 'Depth' field to traverse the inner exceptions flow.

However, not all features in Kibana work just as well with JSON arrays - for instance, including
exception fields on dashboards and visualizations. Therefore, we provide an alternative formatter,  `ExceptionAsObjectJsonFormatter`, which will serialize the exception into the `exception` field as an object with nested `InnerException` properties.

To use it, simply specify it as the `CustomFormatter` when creating the sink:

```csharp
    new OpenSearchSink(new OpenSearchSinkOptions(url)
    {
      CustomFormatter = new ExceptionAsObjectJsonFormatter(renderMessage:true)
    });
```

### JSON `appsettings.json` configuration

To use the OpenSearch sink with _Microsoft.Extensions.Configuration_, for example with ASP.NET Core or .NET Core, use the [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) package. First install that package if you have not already done so:

```powershell
Install-Package Serilog.Settings.Configuration
```

Instead of configuring the sink directly in code, call `ReadFrom.Configuration()`:

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(env.ContentRootPath)
    .AddJsonFile("appsettings.json")
    .Build();

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
```

In your `appsettings.json` file, under the `Serilog` node, :

```json
{
  "Serilog": {
    "WriteTo": [{
        "Name": "OpenSearch",
        "Args": {
          "nodeUris": "http://localhost:9200;http://remotehost:9200/",
          "indexFormat": "custom-index-{0:yyyy.MM}",
          "templateName": "myCustomTemplate",
          "typeName": "myCustomLogEventType",
          "pipelineName": "myCustomPipelineName",
          "batchPostingLimit": 50,
          "batchAction": "Create",
          "period": 2,
          "inlineFields": true,
          "restrictedToMinimumLevel": "Warning",
          "bufferBaseFilename":  "C:/Temp/docker-elk-serilog-web-buffer",
          "bufferFileSizeLimitBytes": 5242880,
          "bufferLogShippingInterval": 5000,
          "bufferRetainedInvalidPayloadsLimitBytes": 5000,
          "bufferFileCountLimit": 31,
          "connectionGlobalHeaders" :"Authorization=Bearer SOME-TOKEN;OtherHeader=OTHER-HEADER-VALUE",
          "connectionTimeout": 5,
          "emitEventFailure": "WriteToSelfLog",
          "queueSizeLimit": "100000",
          "autoRegisterTemplate": true,
          "autoRegisterTemplateVersion": "OSv1",
          "overwriteTemplate": false,
          "registerTemplateFailure": "IndexAnyway",
          "deadLetterIndexName": "deadletter-{0:yyyy.MM}",
          "numberOfShards": 20,
          "numberOfReplicas": 10,
          "templateCustomSettings": [{ "index.mapping.total_fields.limit": "10000000" } ],
          "formatProvider": "My.Namespace.MyFormatProvider, My.Assembly.Name",
          "connection": "My.Namespace.MyConnection, My.Assembly.Name",
          "serializer": "My.Namespace.MySerializer, My.Assembly.Name",
          "connectionPool": "My.Namespace.MyConnectionPool, My.Assembly.Name",
          "customFormatter": "My.Namespace.MyCustomFormatter, My.Assembly.Name",
          "customDurableFormatter": "My.Namespace.MyCustomDurableFormatter, My.Assembly.Name",
          "failureSink": "My.Namespace.MyFailureSink, My.Assembly.Name"
        }
    }]
  }
}
```

See the XML `<appSettings>` example above for a discussion of available `Args` options.

### Handling errors

You have the option to specify how to handle issues with OpenSearch. Since the sink delivers in a batch, it might be possible that one or more events could actually not be stored in the OpenSearch store.
Can be a mapping issue for example. It is hard to find out what happened here. There is an option called *EmitEventFailure* which is an enum (flagged) with the following options:

* WriteToSelfLog, the default option in which the errors are written to the SelfLog.
* WriteToFailureSink, the failed events are send to another sink. Make sure to configure this one by setting the FailureSink option.
* ThrowException, in which an exception is raised.
* RaiseCallback, the failure callback function will be called when the event cannot be submitted to OpenSearch. Make sure to set the FailureCallback option to handle the event.

An example:

```csharp
.WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureSink = new FileSink("./failures.txt", new JsonFormatter(), null)
                })
```

With the AutoRegisterTemplate option the sink will write a default template to OpenSearch. When this template is not there, you might not want to index as it can influence the data quality.
You can use the RegisterTemplateFailure option and set it to one of the following options:

* IndexAnyway; the default option, the events will be send to the server
* IndexToDeadletterIndex; using the deadletterindex format, it will write the events to the deadletter queue. When you fix your template mapping, you can copy your data into the right index.
* FailSink; this will simply fail the sink by raising an exception.

You can also specify an action to do when log row was denied by OpenSearch because of the data (payload) if durable file is specified.
i.e.

```csharp
BufferCleanPayload = (failingEvent, statuscode, exception) =>
                    {
                        dynamic e = JObject.Parse(failingEvent);
                        return JsonConvert.SerializeObject(new Dictionary<string, object>()
                        {
                            { "@timestamp",e["@timestamp"]},
                            { "level","Error"},
                            { "message","Error: "+e.message},
                            { "messageTemplate",e.messageTemplate},
                            { "failingStatusCode", statuscode},
                            { "failingException", exception}
                        });
                    },
```

The IndexDecider didn't work well when durable file was specified so an option to specify BufferIndexDecider is added.
Datatype of logEvent is string
i.e.

```csharp
 BufferIndexDecider = (logEvent, offset) => "log-serilog-" + (new Random().Next(0, 2)),
```

Option BufferFileCountLimit is added. The maximum number of log files that will be retained. including the current log file. For unlimited retention, pass null. The default is 31.
Option BufferFileSizeLimitBytes is added The maximum size, in bytes, to which the buffer log file for a specific date will be allowed to grow. By default `100L * 1024 * 1024` will be applied.

### Targeting AWS OpenSearch

If you are targeting the hosted AWS OpenSearch and the cluster in question has fine-grained access control enabled then you will need to get all requests signed.

Thankfully this is easy to do once you've added the following nuget package `OpenSearch.Net.Auth.AwsSigV4

### Breaking changes

#### Version 1

* Initial version

#### Version 1.1

* Updated dependencies
* Resolved runtime issue when processing invalid information from Opensearch

#### Version 1.2

* Updated dependencies
