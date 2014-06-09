using System;

namespace Dynamo.Interfaces
{
    public interface ILogger
    {
        String LogPath { get; }
        void Log(string message);
        void Log(string message, LogLevel level);
        void Log(string tag, string message);
        void LogError(string error);
        void LogWarning(string warning, WarningLevel level);
        void Log(Exception e);
        void ClearLog();
        string LogText { get; }
        string Warning { get; set; }
    }
}
