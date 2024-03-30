// See https://aka.ms/new-console-template for more information
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using WebApplication1.Model;

public class Program
{

    private readonly IConfiguration _configuration;
    public Program(IConfiguration configuration)
    {
        _configuration = configuration;
    }


    public static async Task Main(string[] args)
    {
        var configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddEnvironmentVariables()
           .Build();
        var program = new Program(configuration);
        await program.RunAsync();
    }

    public async Task RunAsync()
    {
        var settings = _configuration.GetSection("Settings").Get<ConnectionStrings>();
        Console.WriteLine("Hello, World!");
        await using ServiceBusClient client = new ServiceBusClient(settings.ServiceBusConnectionString);
        await using ServiceBusProcessor processor1 = client.CreateProcessor(settings.TopicName, settings.Subs1Name, new ServiceBusProcessorOptions());
        await using ServiceBusProcessor processor2 = client.CreateProcessor(settings.TopicName, settings.Subs2Name, new ServiceBusProcessorOptions());

        async Task MessageHandler(ProcessMessageEventArgs args, string subscriptionName)
        {
            string body = args.Message.Body.ToString();
            Console.WriteLine($"Received message '{body}' from subscription '{subscriptionName}'");
            await args.CompleteMessageAsync(args.Message);
        }

        async Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"An error occurred: {args.Exception}");
        }

        processor1.ProcessMessageAsync += args => MessageHandler(args, settings.Subs1Name);
        processor2.ProcessMessageAsync += args => MessageHandler(args, settings.Subs2Name);

        processor1.ProcessErrorAsync += ErrorHandler;
        processor2.ProcessErrorAsync += ErrorHandler;

        await processor1.StartProcessingAsync();
        await processor2.StartProcessingAsync();

        Console.WriteLine("Press any key to stop receiving messages");
        Console.ReadKey();

        await Task.WhenAll(processor1.StopProcessingAsync(), processor2.StopProcessingAsync());
        Console.WriteLine("Stopped receiving messages");
    }
}