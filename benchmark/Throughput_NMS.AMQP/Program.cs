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
        
        // Drop the first run as it's usually slower due to the JIT compilation
        _ = await Run(connectionFactory, messages);

        for (int i = 0; i < 10; i++)
        {
            var (sendingThroughput, consumingThroughput) = await Run(connectionFactory, messages);
            Console.WriteLine($"Sending throughput: {sendingThroughput:F2} msgs/s | Consuming throughput: {consumingThroughput:F2} msgs/s");
        }
    }
    
    private static async Task<(double sendingThroughput, double consumingThroughput)> Run(NmsConnectionFactory connectionFactory, int messages)
    {
        using var producer = await Producer.CreateAsync(connectionFactory);
        
        var stopwatch = Stopwatch.StartNew();
        await producer.SendMessagesAsync(messages: messages, payloadSize: 1024);
        stopwatch.Stop();
        var sendingThroughput = messages / stopwatch.Elapsed.TotalSeconds;
            
        using var consumer = await Consumer.CreateAsync(connectionFactory);
        stopwatch.Restart();
        await consumer.StartConsumingAsync(messages: messages);
        stopwatch.Stop();
        var consumingThroughput = messages / stopwatch.Elapsed.TotalSeconds;

        return (sendingThroughput, consumingThroughput);
    }
}