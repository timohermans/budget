// From https://github.com/aspnet/Logging/blob/dev/src/Microsoft.Extensions.Logging.Testing/XunitLoggerFactoryExtensions.cs

using System.Text;
using Xunit.Abstractions;

namespace Budget.IntegrationTests.Helpers;

public class XunitLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;
    private readonly LogLevel _minLevel;

    public XunitLoggerProvider(ITestOutputHelper output)
        : this(output, LogLevel.Trace)
    {
    }

    public XunitLoggerProvider(ITestOutputHelper output, LogLevel minLevel)
    {
        _output = output;
        _minLevel = minLevel;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new XunitLogger(_output, categoryName, _minLevel);
    }

    public void Dispose()
    {
    }
}

public class XunitLogger : ILogger
{
    private static readonly string[] NewLineChars = new[] { Environment.NewLine };
    private readonly string _category;
    private readonly LogLevel _minLogLevel;
    private readonly ITestOutputHelper _output;

    public XunitLogger(ITestOutputHelper output, string category, LogLevel minLogLevel)
    {
        _minLogLevel = minLogLevel;
        _category = category;
        _output = output;
    }

    public void Log<TState>(
        LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        // Buffer the message into a single string in order to avoid shearing the message when running across multiple threads.
        var messageBuilder = new StringBuilder();

        var firstLinePrefix = $"| {_category} {logLevel}: ";
        var lines = formatter(state, exception).Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
        messageBuilder.AppendLine(firstLinePrefix + lines.First());

        var additionalLinePrefix = "|" + new string(' ', firstLinePrefix.Length - 1);
        foreach (var line in lines.Skip(1))
        {
            messageBuilder.AppendLine(additionalLinePrefix + line);
        }

        if (exception != null)
        {
            lines = exception.ToString().Split(NewLineChars, StringSplitOptions.RemoveEmptyEntries);
            additionalLinePrefix = "| ";
            foreach (var line in lines)
            {
                messageBuilder.AppendLine(additionalLinePrefix + line);
            }
        }

        // Remove the last line-break, because ITestOutputHelper only has WriteLine.
        var message = messageBuilder.ToString();
        if (message.EndsWith(Environment.NewLine))
        {
            message = message.Substring(0, message.Length - Environment.NewLine.Length);
        }

        try
        {
            _output.WriteLine(message);
        }
        catch (Exception)
        {
            // We could fail because we're on a background thread and our captured ITestOutputHelper is
            // busted (if the test "completed" before the background thread fired).
            // So, ignore this. There isn't really anything we can do but hope the
            // caller has additional loggers registered
        }
    }

    public bool IsEnabled(LogLevel logLevel)
            => logLevel >= _minLogLevel;

    IDisposable ILogger.BeginScope<TState>(TState state)
        => new NullScope();

    private class NullScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}

public static class XunitLoggerFactoryExtensions
{
    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper output)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(output));
        return builder;
    }

    public static ILoggingBuilder AddXunit(this ILoggingBuilder builder, ITestOutputHelper output, LogLevel minLevel)
    {
        builder.Services.AddSingleton<ILoggerProvider>(new XunitLoggerProvider(output, minLevel));
        return builder;
    }

    public static ILoggerFactory AddXunit(this ILoggerFactory loggerFactory, ITestOutputHelper output)
    {
        loggerFactory.AddProvider(new XunitLoggerProvider(output));
        return loggerFactory;
    }

    public static ILoggerFactory AddXunit(this ILoggerFactory loggerFactory, ITestOutputHelper output, LogLevel minLevel)
    {
        loggerFactory.AddProvider(new XunitLoggerProvider(output, minLevel));
        return loggerFactory;
    }
}