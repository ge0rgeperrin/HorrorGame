using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices;

using Steamworks;
using System;

namespace EpicTransport
{
    public class AutoCredentialSetter : MonoBehaviour
    {
        [Header("EOSSDKComponent")]
        [SerializeField] private EOSSDKComponent component;
        [Space(25)]

        [Header("Windows Editor")]
        [SerializeField] private bool winEditorAuthInterface = true;
        [SerializeField] private LoginCredentialType winEditorAuth = LoginCredentialType.AccountPortal;
        [SerializeField] private ExternalCredentialType winEditorConnect = ExternalCredentialType.Epic;

        [Space(15)]

        [Header("Windows Player")]
        [SerializeField] private bool winPlayerAuthInterface = false;
        [SerializeField] private LoginCredentialType winPlayerAuth = LoginCredentialType.ExternalAuth;
        [SerializeField] private ExternalCredentialType winPlayerConnect = ExternalCredentialType.SteamSessionTicket;

        private bool usingSteamworks;
        private HAuthTicket authTicket;

        private void Awake()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                    component.authInterfaceLogin = winEditorAuthInterface;
                    component.authInterfaceCredentialType = winEditorAuth;
                    component.connectInterfaceCredentialType = winEditorConnect;
                    break;

                case RuntimePlatform.WindowsPlayer:
                    component.authInterfaceLogin = winPlayerAuthInterface;
                    component.authInterfaceCredentialType = winPlayerAuth;
                    component.connectInterfaceCredentialType = winPlayerConnect;
                    break;
            }

            if (component.connectInterfaceCredentialType == ExternalCredentialType.SteamSessionTicket)
            {
                usingSteamworks = true;
                StartCoroutine(WaitForSteam());
            }

            EOSSDKComponent.Initialize();
        }

        private IEnumerator WaitForSteam()
        {
            yield return new WaitUntil(() => SteamManager.Initialized);

            EOSSDKComponent.DisplayName = SteamManager.UserData.SteamUsername;
            
            byte[] sessionTicket = new byte[1024];
            SteamNetworkingIdentity sni = new SteamNetworkingIdentity();
            sni.SetSteamID(SteamUser.GetSteamID());

            authTicket = SteamUser.GetAuthSessionTicket(sessionTicket, sessionTicket.Length, out uint ticketLength, ref sni);

            byte[] trimmedTicket = new byte[ticketLength];
            Array.Copy(sessionTicket, trimmedTicket, ticketLength);

            EOSSDKComponent.SetConnectInterfaceCredentialToken(System.Convert.ToBase64String(trimmedTicket));
        }

        private void OnApplicationQuit()
        {
            if (usingSteamworks)
            {
                SteamUser.CancelAuthTicket(authTicket);
                authTicket = HAuthTicket.Invalid;
            }
        }
    }
}
