
using System;

namespace PallonAnticheat
{
    //by rxxyn
    using UnityEngine;
    using Beebyte.Obfuscator;

    /// <summary>
    /// A class that contains tools to handle errors.
    /// </summary>
    [ObfuscateLiterals]
    public class ErrorTools : MonoBehaviour
    {
        public const string DuplicatedScriptInstanceError = "Illegal script instance duplication";
        public const string IllegalActionInterventionError = "Illegal interaction intervened";

        /// <summary>
        /// Quits the game, but logs your given message before it does. Use this for like a fatal PlayFab error or something.
        /// Application.Quit() code: 900
        /// </summary>
        /// <param name="log">the message you want to log before the game closes. </param>
        public static void QuitLog(string log)
        {
            if (string.IsNullOrEmpty(log))
            {
                MonkeLogger.Log(MonkeLogger.LogLevel.Error,"<color=red>Error Tools:</color> The string that was attempted to be logged is NULL! Game will still quit.");
            }
            else
            {
                MonkeLogger.Log(MonkeLogger.LogLevel.Error,"QUITLOG! VIEW LOG BELOW.");
                MonkeLogger.Log(MonkeLogger.LogLevel.Error,log);
            }

            Logger.Log($"\n YOUR GAME SUFFERED A QUIT LOG! MONKEY MISCHIEF AUTOMATICALLY QUITS THE GAME IF ANYTHING GAME-BREAKING OCCURS OR IS FOUND. PLEASE REPORT THIS LOG TO THE DEVELOPERS. \n{log}");

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
            #else
            Application.Quit(900);
            #endif
        }

        public static void SendDevMessageAndQuit(string message)
        {
            throw new NotImplementedException("SendDevMessageAndQuit");
            /*PlayfabController.SendDevMessage(message, callback =>
            {
                string log = $"Quitting game! A developer message was sent before the game quit. Message: {message}";
                MonkeLogger.Log(MonkeLogger.LogLevel.Error,log);
                QuitLog(log);
            });*/
        }

        /// <summary>
        /// Handles a generic PlayFab error
        /// </summary>
        /// <param name="error">The error you want handeled</param>
        public static void HandlePlayFabError(PlayFab.PlayFabError error)
        {
            QuitLog("<color=red>Error Tools:</color> " + error.GenerateErrorReport());
        }
    }
}