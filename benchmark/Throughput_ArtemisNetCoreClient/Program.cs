using System.Diagnostics;
using ActiveMQ.Artemis.Core.Client;

namespace Throughput_ArtemisNetCoreClient;

class Program
{
    static async Task Main(string[] args)
    {
        var endpoint = new Endpoint
        {
            Host = "localhost",
            Port = 61616,
            User = "artemis",
            Password = "artemis"
        };
        
        var messages = 100_000;

        // Drop the first run as it's usually slower due to the JIT compilation
        _ = await Run(endpoint, messages);

        for (int i = 0; i < 10; i++)
        {
            var (sendingThroughput, consumingThroughput) = await Run(endpoint, messages);
            Console.WriteLine($"Sending throughput: {sendingThroughput:F2} msgs/s | Consuming throughput: {consumingThroughput:F2} msgs/s");
        }
    }

    private static async Task<(double sendingThroughput, double consumingThroughput)> Run(Endpoint endpoint, int messages)
    {
        await using var producer = await Producer.CreateAsync(endpoint);
        
        var stopwatch = Stopwatch.StartNew();
        await producer.SendMessages(messages: messages, payloadSize: 1024);
        stopwatch.Stop();
        var sendingThroughput = messages / stopwatch.Elapsed.TotalSeconds;
            
        await using var consumer = await Consumer.CreateAsync(endpoint);
        stopwatch.Restart();
        await consumer.StartConsumingAsync(messages: messages);
        stopwatch.Stop();
        var consumingThroughput = messages / stopwatch.Elapsed.TotalSeconds;

        return (sendingThroughput, consumingThroughput);
    }
}