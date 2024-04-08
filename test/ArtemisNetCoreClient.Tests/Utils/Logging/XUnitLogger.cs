using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace ActiveMQ.Artemis.Core.Client.Tests.Utils.Logging;

public sealed class XUnitLogger(ITestOutputHelper output, string name) : ILogger
{
    private static readonly string LoglevelPadding = ": ";
    private static readonly string MessagePadding;

    static XUnitLogger()
    {
        var logLevelString = GetLogLevelString(LogLevel.Information);
        MessagePadding = new string(' ', logLevelString.Length + LoglevelPadding.Length);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);

        if (!string.IsNullOrEmpty(message) || exception != null)
        {
            WriteMessage(logLevel, name, message, exception);
        }
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel != LogLevel.None;
    }

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        throw new NotSupportedException();
    }

    private void WriteMessage(LogLevel logLevel, string logName, string message, Exception? exception)
    {
        var logLevelString = GetLogLevelString(logLevel);

        output.WriteLine($"{logLevelString}: {logName}");

        if (!string.IsNullOrEmpty(message))
        {
            output.WriteLine($"{MessagePadding}{message}");
        }

        if (exception != null)
        {
            output.WriteLine(exception.ToString());
        }
    }

    private static string GetLogLevelString(LogLevel logLevel) => logLevel switch
    {
        LogLevel.Trace => "trce",
        LogLevel.Debug => "dbug",
        LogLevel.Information => "info",
        LogLevel.Warning => "warn",
        LogLevel.Error => "fail",
        LogLevel.Critical => "crit",
        _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
    };
}