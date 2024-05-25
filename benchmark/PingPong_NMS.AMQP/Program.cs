using Apache.NMS.AMQP;

namespace PingPong_NMS.AMQP;

class Program
{
    static async Task Main()
    {
        var connectionFactory = new NmsConnectionFactory
        {
            UserName = "artemis",
            Password = "artemis"
        };

        for (int i = 0; i < 10; i++)
        {
            using var ping = new Ping(connectionFactory);
            using (new Pong(connectionFactory))
            {
                var stats = await ping.Start(skipMessages: 100, numberOfMessages: 10000);
                Console.WriteLine(stats);
            }
        }
    }
}