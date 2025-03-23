using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices;

using Steamworks;

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
                if (SteamManager.Initialized)
                {
                    EOSSDKComponent.DisplayName = SteamFriends.GetPersonaName();
                    //set up session ticket stuff
                    //EOSSDKComponent.SetConnectInterfaceCredentialToken(SteamUser.)
                }
            }

            EOSSDKComponent.Initialize();
        }
    }
}
