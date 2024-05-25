using ActiveMQ.Artemis.Core.Client;

namespace PingPong_ArtemisNetCoreClient;

class Program
{
    static async Task Main()
    {
        var endpoint = new Endpoint
        {
            Host = "localhost",
            Port = 61616,
            User = "artemis",
            Password = "artemis"
        };

        for (int i = 0; i < 10; i++)
        {
            await using var ping = await Ping.CreateAsync(endpoint);
            await using var pong = await Pong.CreateAsync(endpoint);
            var start = await ping.Start(skipMessages: 100, numberOfMessages: 10000);
            Console.WriteLine(start);
        }
    }
}