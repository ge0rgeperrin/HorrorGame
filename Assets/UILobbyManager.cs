using Epic.OnlineServices.Lobby;
using EpicTransport;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILobbyManager : MonoBehaviour
{
    private List<LobbyDetails> LobbiesFound;
    private List<GameObject> Lobbies;
    public GameObject Prefab;
    public Transform ScrollView;
    private EOSLobby eosLobby;

    const string NameKey = "Name";

    private void Awake()
    {
        eosLobby = GetComponent<EOSLobby>();
        eosLobby.FindLobbiesSucceeded += OnFindLobbiesSuccess;
    }

    private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound)
    {
        LobbiesFound = lobbiesFound;
    }

    public void CreateLobby(string LobbyName)
    {
        eosLobby.CreateLobby(6, LobbyPermissionLevel.Publicadvertised, false, new AttributeData[]
        {
            new AttributeData() { Key = NameKey, Value = LobbyName },
        });
    }

    public void FindLobbies()
    {
        eosLobby.FindLobbies();

        foreach(LobbyDetails lobby in LobbiesFound)
        {
            GameObject temp = Instantiate(Prefab, ScrollView);
            UILobby comp = temp.GetComponent<UILobby>();

            Attribute? lobbyNameAttribute = new Attribute();
            LobbyDetailsCopyAttributeByKeyOptions copyOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = NameKey };
            lobby.CopyAttributeByKey(ref copyOptions, out lobbyNameAttribute);
            comp.Name.text = lobbyNameAttribute.Value.Data.Value.Value.AsUtf8;

            LobbyDetailsCopyInfoOptions options = new LobbyDetailsCopyInfoOptions();
            LobbyDetailsInfo? info;
            lobby.CopyInfo(ref options, out info);
            comp.ID = info.Value.LobbyId;

            //comp.JoinButton.onClick.AddListener(JoinLobby(comp.ID);
        }
    }

    public void JoinLobby(string LobbyId)
    {
        eosLobby.JoinLobbyByID(LobbyId);
    }
}
