using Subterranea;

namespace PallonAnticheat
{
    using System.Collections.Generic;
    using PlayFab;
    using PlayFab.ClientModels;
    using UnityEngine;

    static class Banner
    {
        private static bool loggedIn => LoginManager.LoggedIn;
        private static LoginManager playfab => LoginManager.Instance;
        
        private static BanInfo requestedBan;

        /// <summary> The default scenario for a ban that recieved null details in its request. </summary>
        readonly static BanInfo nullRefBan = new()
        {
            reason = "Illegal scenario: Provided nulldetails to Pallon Ban",
            time = Constants.minimumCheatingBanHours,
            customTags = null
        };
        
        /// <summary> The default scenario for a ban for a user that cheated </summary>
        readonly static BanInfo cheatingBan = new()
        {
            reason = "Cheating",
            time = Constants.minimumCheatingBanHours,
            customTags = null
        };

        public static void RequestBan(string reason, string time, Dictionary<string, string> customTags = default)
        {
            BanInfo thisBan = new()
            {
                reason = reason,
                time = time,
                customTags = customTags
            };

            if (loggedIn)
            {
                Ban(thisBan);
                return;
            }

            requestedBan = thisBan;
        }

        public static void RequestBan(BanInfo banInfo)
        {
            if (loggedIn)
            {
                Ban(banInfo);
                return;
            }

            requestedBan = banInfo;
        }

        static bool executed;

        public static void ExecuteRequestedBan()
        {
            if (requestedBan.reason != null && !executed)
            {
                executed = true;
                Ban(requestedBan, true);
            }
        }

        private static void Ban(BanInfo banInfo, bool isRequest = false)
        {
            if (requestedBan.reason != null && isRequest)
            {
                Ban(nullRefBan);
                return;
            }

            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
            {
                FunctionName = "BanPlayer",
                FunctionParameter = new
                {
                    banreason = banInfo.reason,
                    bantime = banInfo.time,
                    installmode = Application.installMode.ToString(),
                    infodump = PlayerManager.InfoDump
                },
                CustomTags = banInfo.customTags,
            },
            BanSuccess, ErrorTools.HandlePlayFabError);
        }

        private static void BanSuccess(ExecuteCloudScriptResult result)
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            ErrorTools.QuitLog($"{LoginManager.SUID} has been banned!");
#endif
        }

        public struct BanInfo
        {
            public string reason;
            public string time;
            public Dictionary<string, string> customTags;
        }
    }
}