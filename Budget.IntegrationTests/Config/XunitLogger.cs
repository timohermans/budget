﻿using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Budget.Tests.Config;

public class XunitLogger<T>(ITestOutputHelper output) : ILogger<T>, IDisposable
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception, string> formatter)
    {
        output.WriteLine(state?.ToString());
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    IDisposable ILogger.BeginScope<TState>(TState state)
    {
        return this;
    }

    public void Dispose()
    {
    }
}