using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Common
{
    public class ColorConsoleLogger : ILog
    {
        public Guid GetCorrelationId()
        {
            return Guid.NewGuid();
        }

        public void LogCritical(string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.DarkRed, message);
        }

        public void LogError(string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.Red, message);
        }

        public void LogInformation(string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.Green, message);
        }

        public void LogTraceMessage(TraceLevel level, string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.White, message);
        }

        public void LogVerbose(string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.Cyan, message);
        }

        public void LogWarning(string message, string tagId = null)
        {
            ColorConsole.WriteLine(ConsoleColor.Yellow, message);
        }

        public void SetCorrelationId(Guid corrId)
        {
            throw new NotImplementedException();
        }
    }
}
