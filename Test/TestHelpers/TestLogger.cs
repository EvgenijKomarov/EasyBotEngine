using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace EngineTests.TestHelpers
{
    public class TestLogger<T> : ILogger<T>
    {
        private readonly List<string> _messages;
        public TestLogger(List<string> messages) => _messages = messages;

        public IDisposable? BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            try
            {
                var message = formatter(state, exception);
                _messages.Add(message);
            }
            catch
            {
                // ignore formatting errors
            }
        }
    }
}
