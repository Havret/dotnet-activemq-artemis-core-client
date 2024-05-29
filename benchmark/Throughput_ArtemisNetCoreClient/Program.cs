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

        for (int i = 0; i < 10; i++)
        {
            await using var producer = await Producer.CreateAsync(endpoint);
        
            var stopwatch = Stopwatch.StartNew();
            await producer.SendMessagesAsync(messages: messages, payloadSize: 1024);
            stopwatch.Stop();
            Console.WriteLine($"Sending throughput: {messages / stopwatch.Elapsed.TotalSeconds:F2} msgs/s");
            
            await using var consumer = await Consumer.CreateAsync(endpoint);
            stopwatch.Restart();
            await consumer.StartConsumingAsync(messages: messages);
            stopwatch.Stop();
            Console.WriteLine($"Consuming throughput: {messages / stopwatch.Elapsed.TotalSeconds:F2} msgs/s");
        }
    }
}