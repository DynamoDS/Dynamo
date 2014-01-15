namespace Dynamo.UpdateManager
{
    public interface ILoggerWrapper
    {
        void EnableLogging(bool enable);
        void LogInfo(string tag, string data);
        void LogDebug(string tag, string data);
        void LogPerf(string tag, string data);
        void LogError(string tag, string data);
        void FORCE_Log(string tag, string data);
        void CancelProcessing();
    }
}
