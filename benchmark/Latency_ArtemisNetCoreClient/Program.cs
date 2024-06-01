using ActiveMQ.Artemis.Core.Client;

namespace Latency_ArtemisNetCoreClient;

public static class Program
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
        
        var messages = 10_000;

        for (int i = 0; i < 10; i++)
        {
            await using var consumer = await Consumer.CreateAsync(endpoint);
            var startConsumingTask = consumer.StartConsumingAsync(messages: messages);

            await using var producer = await Producer.CreateAsync(endpoint);
            await producer.SendMessagesAsync(messages: messages, payloadSize: 1024);

            var latencies = await startConsumingTask;
            Console.WriteLine($"Latency: avg:{latencies.Average():F2}µs, min:{latencies.Min():F2}µs, max:{latencies.Max():F2}µs");
        }
    }
}