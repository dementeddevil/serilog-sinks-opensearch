﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.File;
using Serilog.Sinks.SystemConsole.Themes;

namespace Serilog.Sinks.Opensearch.Sample
{
    class Program
    {
        private static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", true, true)
            .AddEnvironmentVariables()
            .Build();
       
        static void Main(string[] args)
        {

            // Enable the selflog output
            SelfLog.Enable(Console.Error);
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(theme: SystemConsoleTheme.Literate)
                .WriteTo.Opensearch(new OpensearchSinkOptions(new Uri(Configuration.GetConnectionString("opensearch"))) // for the docker-compose implementation
                {
                    AutoRegisterTemplate = true,
                    OverwriteTemplate = true,
                    DetectOpensearchVersion = true,
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1,
                    NumberOfReplicas = 1,
                    NumberOfShards = 2,
                    //BufferBaseFilename = "./buffer",
                   // RegisterTemplateFailure = RegisterTemplateRecovery.FailSink,
                    FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                    EmitEventFailure = EmitEventFailureHandling.WriteToSelfLog |
                                       EmitEventFailureHandling.WriteToFailureSink |
                                       EmitEventFailureHandling.RaiseCallback,
                    FailureSink = new FileSink("./fail-{Date}.txt", new JsonFormatter(), null, null)
                })
                .CreateLogger();

            Log.Information("Hello, world!");
         
            int a = 10, b = 0;
            try
            {
                Log.Debug("Dividing {A} by {B}", a, b);
                Console.WriteLine(a / b);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Something went wrong");
            }

            // Introduce a failure by storing a field as a different type
            Log.Debug("Reusing {A} by {B}", "string", true);

            Log.CloseAndFlush();
            Console.WriteLine("Press any key to continue...");
            while (!Console.KeyAvailable)
            {
                Thread.Sleep(500);
            }
        }

      
    }
}
