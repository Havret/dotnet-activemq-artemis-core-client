using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests.Utils.Logging;

public sealed class XUnitLoggerFactory : ILoggerFactory
{
    private readonly ITestOutputHelper _output;

    public XUnitLoggerFactory(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XUnitLogger(_output, categoryName);
    }

    void IDisposable.Dispose()
    {
    }

    void ILoggerFactory.AddProvider(ILoggerProvider provider)
    {
    }
}