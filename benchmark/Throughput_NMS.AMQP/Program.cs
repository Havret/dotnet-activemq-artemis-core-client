using System.Diagnostics;
using Apache.NMS.AMQP;

namespace Throughput_NMS_AMQP;

class Program
{
    static async Task Main(string[] args)
    {
        var connectionFactory = new NmsConnectionFactory
        {
            UserName = "artemis",
            Password = "artemis"
        };
                
        var messages = 100_000;

        for (int i = 0; i < 10; i++)
        {
            using var producer = await Producer.CreateAsync(connectionFactory);
        
            var stopwatch = Stopwatch.StartNew();
            await producer.SendMessagesAsync(messages: messages, payloadSize: 1024);
            stopwatch.Stop();
            Console.WriteLine($"Sending throughput: {messages / stopwatch.Elapsed.TotalSeconds:F2} msgs/s");
        
            using var consumer = await Consumer.CreateAsync(connectionFactory);
            stopwatch.Restart();
            await consumer.StartConsumingAsync(messages: messages);
            stopwatch.Stop();
            Console.WriteLine($"Consuming throughput: {messages / stopwatch.Elapsed.TotalSeconds:F2} msgs/s");
        }
    }
}