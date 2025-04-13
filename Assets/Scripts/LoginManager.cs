using TMPro;

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

    public TMP_Text DebugText;
    
    public static string SUID => UserData.SUID;
    
    public static userData UserData;
    public static helper Helpers;

    public static bool LoggedIn;

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
        LoggedIn = false;
        
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
            StartCoroutine(AuthenticateWithSteam());
        }
    }
    
    private IEnumerator AuthenticateWithSteam()
    {
        DebugText.text = "Logging in... (1/3)";
        yield return new WaitUntil(() => SteamManager.Initialized);
        
        DebugText.text = "Logging in... (2/3)";
        yield return new WaitUntil(() => EOSSDKComponent.Initialized);
        DebugText.text = "Logging in... (3/3)";

        yield return new WaitForSeconds(2f);
        DebugText.text = "Validating...";
        
        MonkeLogger.Log("Logging in with PlayFab. EOS has been initalized.");
        
        yield return new WaitForSeconds(5f);
        
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
        LoggedIn = true;
        DebugText.text = string.Empty;
        MonkeLogger.Log($"Successfully logged into Subterranea! SUID: {result.PlayFabId}");
        
        UserData.LoadData(result);
        PlayFabClientAPI.LinkCustomID(new LinkCustomIDRequest {CustomId = EOSSDKComponent.LocalUserProductIdString}, msg => { MonkeLogger.Log("Linked EOS PUID to PlayFab Custom ID!"); }, error => {});
    }

    private void OnLoginError(PlayFabError error)
    {
        LoggedIn = false;
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