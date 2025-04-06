namespace Subterranea
{
    using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using EpicTransport;
using PlayFab;
using PlayFab.ClientModels;
using PallonAnticheat;
using Steamworks;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public static LoginManager Instance { get; private set; }

    public static string SUID => UserData.SUID;
    
    public static userData UserData;
    public static helper Helpers;

    public static bool LoggedIn;
    public static bool SteamInitalized => SteamManager.Initialized;

    public static bool IsOnPC
    {
        get
        {
#if PLATFORM_STANDALONE_WIN
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsOnQuest
    {
        get
        {
#if UNITY_ANDROID
        return true;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        PallonAnticheat.Logger.ConfigureLogger();
    }

    private void Start()
    {
        Instance = this;
        DecideAuthentication();
    }

    private void DecideAuthentication()
    {
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            StartCoroutine(AuthenticateWithSteam());
        }
    }
    
    private IEnumerator AuthenticateWithSteam()
    {
        yield return new WaitUntil(() => EOSSDKComponent.Initialized);
        
        MonkeLogger.Log("Logging in with PlayFab. EOS has been initalized.");
        
        if (Application.isEditor)
            MonkeLogger.Log("You are on the editor! Please wait 15 seconds to eliminate ticket invalidation.");
        
        yield return new WaitForSeconds(Application.isEditor ? 15f : 5f);

        yield return new WaitUntil(() => SteamInitalized);
        
        if (!SteamManager.Initialized)
        {
            yield break;
        }
        
        var request = new LoginWithSteamRequest()
        {
            CreateAccount = true,
            SteamTicket = GetSteamAuthenticationTicket(),
            TicketIsServiceSpecific = false,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams { GetUserAccountInfo = true, GetPlayerProfile = true, GetTitleData = true, GetUserInventory = true, GetUserData = true },
        };

        MonkeLogger.Log("Requesting authentication for login.");
        MonkeLogger.Log(log: "Requesting Steam authentication through PlayFab.", level: MonkeLogger.LogLevel.Warning);
        PlayFabClientAPI.LoginWithSteam(request, OnLoginSuccess, OnLoginError);
    }

    private string GetSteamAuthenticationTicket()
    {
        try
        {
            byte[] ticketBytes = new byte[1024];
            SteamNetworkingIdentity sni = new();
            sni.SetSteamID(SteamManager.UserData.SteamID);
            var authTicket = SteamUser.GetAuthSessionTicket(ticketBytes, ticketBytes.Length, out uint ticketSize, ref sni);
            Array.Resize(ref ticketBytes, (int)ticketSize);
            StringBuilder sb = new();
            foreach (byte b in ticketBytes)
            {
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }
        catch(System.Exception e)
        {
            MonkeLogger.LogException(e, "Getting Steam Auth Ticket");
        }
        return string.Empty;
    }

    private void OnLoginSuccess(LoginResult result)
    {
        UserData.LoadData(result);
    }

    private void OnLoginError(PlayFabError error)
    {
        
    }

    #region Title

    public void ChangePlayFabDisplayName(string name)
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = name}, msg => {}, err => {});
    }
    
    #endregion
}

public struct userData
{
    public string SUID;

    public void LoadData(LoginResult result)
    {
        SUID = result.PlayFabId;
    }
}

public struct helper
{
    public bool IsLocalPlayer(string SUID)
    {
        return SUID == LoginManager.UserData.SUID;
    }
}

}