using PlayFab;
using Subterranea;

namespace PallonAnticheat
{
    using System;
    using System.Globalization;
    using System.IO;
    using UnityEngine;

    /// <summary> The logger is configured at runtime. </summary>
    public static class Logger
    {
        public static string currentLog = new("");

        private static string logName;
        private static string logFolder;
        private static string logFile;

        private static bool loggerInit = false;

        /// <summary> Logs a line </summary>
        /// <param name="log"></param>
        public static void Log(string log)
        {
            using (StreamWriter writer = new StreamWriter(logFile, true))
            {
                writer.WriteLine($"{PlayerManager.LocalDateTimeNow:hh:mm:ss tt}: {log}");
            }
        }
        
        public static string ReturnPlayFabError(PlayFabError error)
        {
            return $"PlayFab Error! Error Code: {error.Error}\nError Report: {error.GenerateErrorReport()}\nError Message: {error.ErrorMessage}";
        }
        
        /// <summary> Configures the Logger. </summary>
        public static void ConfigureLogger()
        {
            try
            {
                if (loggerInit)
                {
                    return;
                }

                var path = Path.GetDirectoryName(Application.dataPath);
                
                logName = $"{UnityEngine.Application.productName.ToLower()}_log_{PlayerManager.LocalDateTimeNow:yyyy-MM-dd_hh-mm-ss-tt}";
                logFolder = Path.Combine(Directory.GetParent((LoginManager.IsOnPC || UnityEngine.Application.isEditor) ? Path.GetDirectoryName(Application.dataPath): Application.persistentDataPath).FullName, "Logs");
                logFile = $"{logFolder}/{logName}.txt";

                if (!Directory.Exists(logFolder))
                {
                    Directory.CreateDirectory(logFolder);
                }

                loggerInit = true;
            }
            catch (Exception e)
            {
                MonkeLogger.LogException(e, "Configuring Logger");
            }
        }
    }
}