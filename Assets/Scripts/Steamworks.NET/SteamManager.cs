

namespace Steamworks
{
    
#if !(UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX)
#define DISABLESTEAMWORKS
#endif
    
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Serialization;
using System.Text.RegularExpressions;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using EpicTransport;
using TMPro;using PallonAnticheat;
using Constants = PallonAnticheat.Constants;


[DisallowMultipleComponent]
public class SteamManager : MonoBehaviour
{
#if !DISABLESTEAMWORKS
    [SerializeField] private bool EOSLogin;
    [SerializeField] private bool DebugWebApiTicket;
    
    [Space(20)]
    [SerializeField] private TMP_Text SteamDebugText;
    
    public static string SteamID => UserData.s_SteamID; 
    public static userData UserData;
    
    protected Callback<AvatarImageLoaded_t> avatarLoaded;
    protected Callback<GetTicketForWebApiResponse_t> getTicket;

    [System.Serializable]
    public struct userData
    {
        public CSteamID SteamID;
        /// <summary> The user's Steam ID in string format </summary>
        public string s_SteamID => SteamID.m_SteamID.ToString();
        public string SteamUsername;
        public string LanguageCode;
        public AppId_t AppID;

        public readonly string GetSteamUsername()
        {
            if (Initialized && !string.IsNullOrEmpty(SteamUsername))
                return SteamUsername;

            if (Initialized && string.IsNullOrEmpty(SteamUsername))
            {
                string username = SteamFriends.GetPersonaName();
                username = Regex.Replace(username, @"[^A-Za-z1-9]", "").ToUpper();
                return username;
            }

            return PlayerManager.DesperateFallback();
        }
    }
    
    [HideInInspector] public string steamAppIdPath;

    protected static bool s_EverInitialized = false;

    protected static SteamManager s_instance;
    protected static SteamManager Instance
    {
        get
        {
            if (s_instance == null)
            {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            else
            {
                return s_instance;
            }
        }
    }

    public static SteamManager instance
    {
        get
        {
            return Instance;
        }
    }

    protected bool m_bInitialized = false;
    public static bool Initialized
    {
        get
        {
            return Instance.m_bInitialized;
        }
    }

    protected SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook;

    [AOT.MonoPInvokeCallback(typeof(SteamAPIWarningMessageHook_t))]
    protected static void SteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText)
    {
        Debug.LogWarning(pchDebugText);
    }

#if UNITY_2019_3_OR_NEWER
    // In case of disabled Domain Reload, reset static members before entering Play Mode.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void InitOnPlayMode()
    {
        s_EverInitialized = false;
        s_instance = null;
    }
#endif

    protected virtual void Awake()
    {
        // Only one instance of SteamManager at a time!
        if (s_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        SteamDebugText.text = $"Logged In: {Initialized}\nSteam Username: null\n\nApp ID: 000";

        string steamAppIdPath = string.Empty;
        string steamAppIdPrefix = "/steam_appid.txt";
        if (Application.isEditor)
        {
            #if UNITY_EDITOR
            steamAppIdPath = Path.GetDirectoryName(EditorApplication.applicationPath) + steamAppIdPrefix;
#endif
        }
        else
        {
            steamAppIdPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + steamAppIdPrefix;
        }
       
        MonkeLogger.Log($"Writing Steam app ID {PallonAnticheat.Constants.steamAppId} in path {steamAppIdPath}");
        //overwrites anything in steam app id file with app id
        File.WriteAllText(steamAppIdPath, PallonAnticheat.Constants.steamAppId);

        if (s_EverInitialized)
        {
            // This is almost always an error.
            // The most common case where this happens is when SteamManager gets destroyed because of Application.Quit(),
            // and then some Steamworks code in some other OnDestroy gets called afterwards, creating a new SteamManager.
            // You should never call Steamworks functions in OnDestroy, always prefer OnDisable if possible.
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }
        

        // We want our SteamManager Instance to persist across scenes.
        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test())
        {
            Debug.LogError("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test())
        {
            Debug.LogError("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try
        {
            // If Steam is not running or the game wasn't started through Steam, SteamAPI_RestartAppIfNecessary starts the
            // Steam client and also launches this game again if the User owns it. This can act as a rudimentary form of DRM.

            // Once you get a Steam AppID assigned by Valve, you need to replace AppId_t.Invalid with it and
            // remove steam_appid.txt from the game depot. eg: "(AppId_t)480" or "new AppId_t(480)".
            // See the Valve documentation for more information: https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
            
            AppId_t Subterranea = new AppId_t(uint.Parse(PallonAnticheat.Constants.steamAppId));
            if (SteamAPI.RestartAppIfNecessary(Subterranea))
            {
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e)
        { // We catch this exception here, as it will be the first occurrence of it.
            Debug.LogError("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location. Refer to the README for more details.\n" + e, this);

            Application.Quit();
            return;
        }

        // Initializes the Steamworks API.
        // If this returns false then this indicates one of the following conditions:
        // [*] The Steam client isn't running. A running Steam client is required to provide implementations of the various Steamworks interfaces.
        // [*] The Steam client couldn't determine the App ID of game. If you're running your application from the executable or debugger directly then you must have a [code-inline]steam_appid.txt[/code-inline] in your game directory next to the executable, with your app ID in it and nothing else. Steam will look for this file in the current working directory. If you are running your executable from a different directory you may need to relocate the [code-inline]steam_appid.txt[/code-inline] file.
        // [*] Your application is not running under the same OS user context as the Steam client, such as a different user or administration access level.
        // [*] Ensure that you own a license for the App ID on the currently active Steam account. Your game must show up in your Steam library.
        // [*] Your App ID is not completely set up, i.e. in Release State: Unavailable, or it's missing default packages.
        // Valve's documentation for this is located here:
        // https://partner.steamgames.com/doc/sdk/api#initialization_and_shutdown
        m_bInitialized = SteamAPI.Init();
        if (m_bInitialized)
        {
            AssignUserValues();
        }
        if (!m_bInitialized)
        {
            Debug.LogError("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation or the comment above this line for more information.", this);

            return;
        }

        s_EverInitialized = true;
    }

    // This should only ever get called on first load and after an Assembly reload, You should never Disable the Steamworks Manager yourself.
    protected virtual void OnEnable()
    {
        if (s_instance == null)
        {
            s_instance = this;
        }

        if (!m_bInitialized)
        {
            return;
        }

        if (m_SteamAPIWarningMessageHook == null)
        {
            // Set up our callback to receive warning messages from Steam.
            // You must launch with "-debug_steamapi" in the launch args to receive warnings.
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(SteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    // OnApplicationQuit gets called too early to shutdown the SteamAPI.
    // Because the SteamManager should be persistent and never disabled or destroyed we can shutdown the SteamAPI here.
    // Thus it is not recommended to perform any Steamworks work in other OnDestroy functions as the order of execution can not be garenteed upon Shutdown. Prefer OnDisable().
    protected virtual void OnDestroy()
    {
        if (s_instance != this)
        {
            return;
        }

        s_instance = null;

        if (!m_bInitialized)
        {
            return;
        }

        SteamAPI.Shutdown();
    }

    protected virtual void OnDisable()
    {
        MonkeLogger.Log(log: "Why is the SteamManager being disabled?", level: MonkeLogger.LogLevel.Error);
        this.enabled = true;
    }

    protected virtual void Update()
    {
        if (!m_bInitialized)
        {
            return;
        }

        // Run Steam client callbacks
        SteamAPI.RunCallbacks();
    }
#else
    public static bool Initialized
    {
        get
        {
            return false;
        }
    }
#endif 

#if !DISABLESTEAMWORKS
    private void AssignUserValues()
    {
        UserData.SteamUsername = UserData.GetSteamUsername();
        UserData.SteamID = SteamUser.GetSteamID();
        UserData.LanguageCode = SteamApps.GetCurrentGameLanguage();
        UserData.AppID = SteamUtils.GetAppID();
        getTicket = Callback<GetTicketForWebApiResponse_t>.Create(OnAuthWebApiTicketLoaded);
        avatarLoaded = Callback<AvatarImageLoaded_t>.Create(OnAvatarLoaded);
        MonkeLogger.Log($"Authenticated with SteamID {UserData.s_SteamID} (Username: {UserData.SteamUsername}) Steam login is OK!");
        
        SteamDebugText.text = $"Logged In: {Initialized}" +
                         $"\nSteam Username: {UserData.SteamUsername}" +
                         $"\nSDK Running In App ID: {UserData.AppID.m_AppId.ToString()}";
        
        LoginIntoEOS();
    }

    private void LoginIntoEOS()
    {
        if (EOSLogin)
        {
            MonkeLogger.Log("Logging into EOS with Steam. Requesting Web API Ticket");
            GetWebApiTicket();
        }
    }
    
    private void GetWebApiTicket()
    {
        MonkeLogger.Log("Getting Web API Ticket.");
        SteamUser.GetAuthTicketForWebApi("epiconlineservices");
    }

    private void RequestEOSLogin(string ticket)
    {
        string ticketLog = (DebugWebApiTicket) ? $"Ticket is valid. Ticket: {ticket}" : "Ticket is valid.";
        MonkeLogger.Log($"Requesting a Steam login with EOS. {ticketLog}");
        EOSSDKComponent.SetConnectInterfaceCredentialToken(ticket);
        EOSSDKComponent.Initialize();
    }

    private void OnAuthWebApiTicketLoaded(GetTicketForWebApiResponse_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            MonkeLogger.Log(MonkeLogger.LogLevel.Error,$"Failed to get Web API ticket! Error: {callback.m_eResult}");
            return;
        }
        
        MonkeLogger.Log("Received Web API Ticket successfully!");
        
        byte[] bytes = callback.m_rgubTicket;
        StringBuilder sb = new();
        foreach (var b in bytes)
            sb.AppendFormat("{0:x2}", b);
        string ticket = sb.ToString();
        
        RequestEOSLogin(ticket);
    }
    
    private void OnAvatarLoaded(AvatarImageLoaded_t callback)
    {
        MonkeLogger.Log("The Player's steam avatar has loaded!");
    }

    #region Achievements
    
    /// <summary> Achieves a achievement. </summary>
    public static void Achieve(string achievement)
    {
        if (!Initialized)
            return;
        SteamUserStats.SetAchievement(achievement);
        SendAchievementDataToServer();
    }

    /// <summary> Adds a count/stat value to a achievement. </summary>
    public static void AddCount(string achievement, ulong count)
    {
        if (!Initialized)
            return;

        SendAchievementDataToServer();
    }

    public static void SaveStat(string stat, ulong count)
    {
        if (!Initialized)
            return;

        SteamUserStats.SetStat(stat, count);
        SendAchievementDataToServer();
    }

    /// <summary> Achieves only if the given condition is met. </summary>
    public static void Achieve(string achievement, bool condition)
    { 
        if (!Initialized)
            return;
        if (condition)
        {
            Achieve(achievement);
        }
    }

    private static void SendAchievementDataToServer() => SteamUserStats.StoreStats();

    #endregion
    
    public static Texture2D GetSteamPFP(CSteamID steamID)
    {
        int iImage = SteamFriends.GetLargeFriendAvatar(steamID);

        if (iImage == -1)
        {
            MonkeLogger.Log(level: MonkeLogger.LogLevel.Illegal, log: "Steam returned a PFP value of -1. This is illegal.");
            return null;
        }

        if (SteamUtils.GetImageSize(iImage, out uint width, out uint height))
        {
            byte[] image = new byte[width * height * 4];

            if (SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4)))
            {
                Texture2D texture = new((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
                return texture;
            }
        }

        return null;
    }
#endif
}
}