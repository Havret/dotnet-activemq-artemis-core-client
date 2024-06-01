using System.Diagnostics;
using Apache.NMS.AMQP;

namespace Latency_NMS.AMQP;

public static class Program
{
    static async Task Main(string[] args)
    {
        var connectionFactory = new NmsConnectionFactory
        {
            UserName = "artemis",
            Password = "artemis"
        };
                
        var messages = 10_000;

        for (int i = 0; i < 10; i++)
        {
            using var consumer = await Consumer.CreateAsync(connectionFactory);
            var startConsumingTask = consumer.StartConsumingAsync(messages: messages);

            using var producer = await Producer.CreateAsync(connectionFactory);
        
            await producer.SendMessagesAsync(messages: messages, payloadSize: 1024);

            var latencies = await startConsumingTask;
            Console.WriteLine($"Latency: avg:{latencies.Average():F2}µs, min:{latencies.Min():F2}µs, max:{latencies.Max():F2}µs");
        }
    }
}