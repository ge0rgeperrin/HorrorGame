namespace PallonAnticheat
{
    using PlayFab;
    using UnityEngine;

    /// <summary>
    /// Made by rxxyn
    /// </summary>
    public static class MonkeLogger
    {
        public static void Log(LogLevel level, MonoBehaviour source, string log, Object sceneSource = default)
        {
            Log(level, log + $" ({source})", sceneSource);
        }

        public static void Log(string log, Object source = default)
        {
            Log(LogLevel.Log, null, log, source);
        }

        public static void Log(LogLevel level, string log, Object source = default)
        {
            switch (level)
            {
                case LogLevel.Log:
                    Debug.Log($"[MonkeLogger] - <color=silver>{level}:</color> {log}", source);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning($"[MonkeLogger] - <color=yellow>{level}:</color> {log}", source);
                    break;
                case LogLevel.Error:
                    Debug.LogError($"[MonkeLogger] - <color=red>{level}:</color> {log}", source);
                    break;
                case LogLevel.Illegal:
                    Debug.LogError($"[MonkeLogger] - <color=red>[MonkeLogger] An illegal action was caught and logged. Info logged below.</color>");
                    Debug.LogError($"[MonkeLogger] - <color=red>{level}:</color> {log}", source);
                    break;
            }
            
            PallonAnticheat.Logger.Log($"Log: [{level.ToString()}]: {log}");
        }

        public static void LogException(System.Exception e, string action, bool dump = false)
        {
            if (!dump)
            {
                Log(level: LogLevel.Error, $"There was a exception with action {action}! Error {e.Source}: {e.Message}, {e.StackTrace}: {e.InnerException}");
            }
        }

        public static void LogAssert(string log, bool condition)
        {
            Debug.Assert(condition, $"[MonkeLogger] Assertion Logged!- {log}");
        }

        public static void LogPlayFabError(PlayFabError error)
        {
            Log(LogLevel.Error,$"A PlayFab error has been caught and logged! Details line breaked below: {error.Error}: {error.ErrorMessage}, {error.GenerateErrorReport()}: {error.HttpCode}");
        }

        public enum LogLevel
        {
            Log,
            Warning,
            Error,
            Illegal
        }
    }
}