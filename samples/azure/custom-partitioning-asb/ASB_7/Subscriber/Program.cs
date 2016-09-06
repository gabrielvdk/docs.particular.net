﻿using System;
using System.Threading.Tasks;
using NServiceBus;

class Program
{
    static void Main()
    {
        MainAsync().GetAwaiter().GetResult();
    }

    static async Task MainAsync()
    {
        Console.Title = "Samples.ASB.Partitioning.Subscriber";
        Console.WriteLine("Which connectionstring shall I subscribe to [1, 2]:");
        var instance = Console.ReadLine();
        var endpointConfiguration = new EndpointConfiguration("Samples.ASB.Partitioning.Subscriber");
        var transport = endpointConfiguration.UseTransport<AzureServiceBusTransport>();
        var connectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString" + instance);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new Exception($"Could not read the 'AzureServiceBus.ConnectionString{instance}' environment variable. Check the sample prerequisites.");
        }
        transport.ConnectionString(connectionString);
        var topology = transport.UseTopology<ForwardingTopology>();
        
        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.UseSerialization<JsonSerializer>();
        endpointConfiguration.EnableInstallers();
        endpointConfiguration.UsePersistence<InMemoryPersistence>();
        endpointConfiguration.Recoverability().Delayed(settings => settings.NumberOfRetries(0));

        var endpointInstance = await Endpoint.Start(endpointConfiguration)
            .ConfigureAwait(false);
        try
        {
            Console.WriteLine("Subscriber is ready to receive events on namespace" + instance);
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }
        finally
        {
            await endpointInstance.Stop()
                .ConfigureAwait(false);
        }
    }
}