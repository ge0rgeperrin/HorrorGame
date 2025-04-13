namespace Subterranea
{
    using System;
using System.Collections;
using System.Collections.Generic;
using Epic.OnlineServices.Lobby;
using EpicTransport;
using Mirror;
using PallonAnticheat;
using Subterranea;
using TMPro;
using UnityEngine;
using Attribute = Epic.OnlineServices.Lobby.Attribute;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance;
    
    public GameObject PlayMenu;
    
    [Header("Lobby Shit")]
    public GameObject LobbyMenu;
    public GameObject LobbySearchResultPrefab;
    public GameObject LobbySearchResultContainer;
    public TMP_Text NoLobbiesFoundText;

    public EOSLobby eosLobby => EOSSDKComponent.Instance.eosLobby;

    private void Start()
    {
        Instance = this;
        StartCoroutine(MainMenuSequence());

        eosLobby.FindLobbiesSucceeded += OnFindLobbiesSucceeded;
        eosLobby.FindLobbiesFailed += OnFindLobbiesFailed;
        eosLobby.JoinLobbySucceeded += OnJoinLobbySucceeded;
        eosLobby.JoinLobbyFailed += OnJoinLobbyFailed;
    }

    private IEnumerator MainMenuSequence()
    {
        LobbyMenu.SetActive(false);
        if (LobbySearchResultContainer.transform.childCount >= 1)
        {
            for (int i = 0; i < LobbySearchResultContainer.transform.childCount; i++)
            {
                Transform child = LobbySearchResultContainer.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }
        NoLobbiesFoundText.gameObject.SetActive(false);
        PlayMenu.SetActive(false);
        yield return new WaitUntil(() => LoginManager.LoggedIn);
        PlayMenu.SetActive(true);
    }
    
    #region Lobby Menu
    
    public void FindLobbies(uint lobbies)
    {
        NoLobbiesFoundText.gameObject.SetActive(false);
        MonkeLogger.Log($"Searching for lobbies... Requested: {lobbies}");
        eosLobby.FindLobbies(lobbies);
    }

    private void OnFindLobbiesSucceeded(List<LobbyDetails> lobbies)
    {
        MonkeLogger.Log($"Found lobbies! Count: {lobbies.Count}");
        
        if (LobbySearchResultContainer.transform.childCount >= 1)
        {
            for (int i = 0; i < LobbySearchResultContainer.transform.childCount; i++)
            {
                Transform child = LobbySearchResultContainer.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }

        if (lobbies.Count >= 1)
        {
            NoLobbiesFoundText.gameObject.SetActive(false);
            foreach (LobbyDetails lobby in lobbies)
            {
                GameObject lobbySearchResultObject = Instantiate(LobbySearchResultPrefab);
                lobbySearchResultObject.transform.SetParent(LobbySearchResultContainer.transform);
                LobbySearchResult lobbySearchResult = lobbySearchResultObject.GetComponent<LobbySearchResult>();
                lobbySearchResult.LoadLobbyInfo(lobby);
            }
        }
        else
        {
            NoLobbiesFoundText.gameObject.SetActive(true);
        }
    }

    private void OnFindLobbiesFailed(string error)
    {
        MonkeLogger.Log(MonkeLogger.LogLevel.Error,$"Finding lobbies failed! Error {error}");
        
        if (LobbySearchResultContainer.transform.childCount >= 1)
        {
            for (int i = 0; i < LobbySearchResultContainer.transform.childCount; i++)
            {
                Transform child = LobbySearchResultContainer.transform.GetChild(i);
                Destroy(child.gameObject);
            }
        }
    }

    private void OnJoinLobbySucceeded(List<Attribute> lobbyAttributes)
    {
        MonkeLogger.Log("Joined lobby successfully!");
    }

    private void OnJoinLobbyFailed(string error)
    {
        MonkeLogger.Log(MonkeLogger.LogLevel.Error,$"Joining lobby failed! Error {error}");
    }
    
    #endregion
}
}