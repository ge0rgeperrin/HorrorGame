using System;
using Epic.OnlineServices.Lobby;
using UnityEngine;
using System.Collections.Generic;
using EpicTransport;
using Mirror;
using Attribute = Epic.OnlineServices.Lobby.Attribute;
using System.Collections;
using UnityEngine.Networking;

[RequireComponent(typeof(EOSLobby))]
public class EOSLobbyHUD : MonoBehaviour 
{
    public EOSLobby _eosLobby;

    [Header("Network Manager")]
    public NetworkManager manager;

    [Header("Lobby Settings")]
    public string lobbyName = "My Lobby";
    public float FPS;
    public string playerID = "";
    public string lobbyID = "";
    public int maxPlayers = 5;
    public bool vis = true;
    public bool latejoin = true;

    private string connectionState = "Not logged in";
    private string loadingtext = "";
    private bool _showLobbyList = false;
    private bool _showPlayerList = false;

    private List<LobbyDetails> _foundLobbies = new List<LobbyDetails>();
    private List<Attribute> _lobbyData = new List<Attribute>();

    public const string LobbyNameKey = "LobbyName";
    public const string HostNameKey = "HostName";
    public const string VisKey = "RoomVis";
    public const string VersionKey = "HostVersion";

    private void Awake()
    {
        _eosLobby = GetComponent<EOSLobby>();
    }

    private void Start()
    {
#if UNITY_EDITOR
        StartCoroutine(FramesPerSecond());
        InvokeRepeating("Tick", 1f, 1f);
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (EosTransport.active.ClientConnected())
        {
            connectionState = "In Lobby";
        }
        else if (EOSSDKComponent.Initialized)
        {
            connectionState = "Logged in";
        }
        else
        {
            connectionState = "Not logged in";
        }
#endif
    }

#if UNITY_EDITOR
    private IEnumerator FramesPerSecond()
    {
        while (true)
        {
            float fps = (int)(1f / Time.deltaTime);
            FPS = fps;

            yield return new WaitForSeconds(0.2f);
        }
    }
#endif

#if UNITY_EDITOR
    private void Tick()//1sec tick
    {
        if (_eosLobby.ConnectedToLobby)
        {
            LobbyDetailsCopyInfoOptions copyInfoOptions = new LobbyDetailsCopyInfoOptions { };
            LobbyDetailsInfo? detailsinfo;
            _eosLobby.ConnectedLobbyDetails.CopyInfo(ref copyInfoOptions, out detailsinfo);
            lobbyID = detailsinfo.Value.LobbyId;
        }
    }
#endif

    public void CreateLobbyy(string LobbyName, int MaxPlayers, bool visible, bool latejoin)
    {
        _eosLobby.CreateLobby((uint)MaxPlayers, LobbyPermissionLevel.Publicadvertised, false,
            new AttributeData[]
            {
                new AttributeData
                {
                    Key = LobbyNameKey, Value = LobbyName
                },
                new AttributeData
                {
                    Key = HostNameKey, Value = EOSSDKComponent.DisplayName,
                },
                new AttributeData
                {
                    Key = VisKey, Value = visible //true means visible, false means private
                },
                new AttributeData
                {
                    Key = VersionKey, Value = Application.version
                },

            });
    }

    //register events
    private void OnEnable()
    {
        //subscribe to events
        _eosLobby.CreateLobbySucceeded += OnCreateLobbySuccess;
        _eosLobby.JoinLobbySucceeded += OnJoinLobbySuccess;
        _eosLobby.FindLobbiesSucceeded += OnFindLobbiesSuccess;
        _eosLobby.LeaveLobbySucceeded += OnLeaveLobbySuccess;
        _eosLobby.CreateLobbyFailed += CreateLobbyFailed;
#if UNITY_EDITOR
        //Debug.Log("CapuHUD: subcribed to events");
#endif
    }

    private void CreateLobbyFailed(string errorMessage)
    {
        Debug.LogError("Failed to create lobby: " + errorMessage);
    }

    //deregister events
    private void OnDisable()
    {
        //unsubscribe from events
        _eosLobby.CreateLobbySucceeded -= OnCreateLobbySuccess;
        _eosLobby.JoinLobbySucceeded -= OnJoinLobbySuccess;
        _eosLobby.FindLobbiesSucceeded -= OnFindLobbiesSuccess;
        _eosLobby.LeaveLobbySucceeded -= OnLeaveLobbySuccess;
        //s.OnTranscriptionResult -= TransCame;


        //Debug.Log("CapuHUD: unsubcribed to events");
    }

    //when the lobby is successfully created, start the host
    private void OnCreateLobbySuccess(List<Attribute> attributes)
    {
        _lobbyData = attributes;
        _showPlayerList = true;
        _showLobbyList = false;

        manager.StartHost();
    }

    //when the user joined the lobby successfully, set network address and connect
    private void OnJoinLobbySuccess(List<Attribute> attributes)
    {
        _lobbyData = attributes;
        _showPlayerList = true;
        _showLobbyList = false;

        Attribute hostAddressAttribute = attributes.Find((x) => x.Data.HasValue && x.Data.Value.Key == EOSLobby.hostAddressKey);
        if (!hostAddressAttribute.Data.HasValue)
        {
            Debug.LogError("Host address not found in lobby attributes. Cannot connect to host.");
            return;
        }

        manager.networkAddress = hostAddressAttribute.Data.Value.Value.AsUtf8;
        manager.StartClient();


    }

    //callback for FindLobbiesSucceeded
    private void OnFindLobbiesSuccess(List<LobbyDetails> lobbiesFound)
    {
        _foundLobbies = lobbiesFound;
        _showPlayerList = false;
        _showLobbyList = true;
    }

    //when the lobby was left successfully, stop the host/client
    private void OnLeaveLobbySuccess()
    {
        manager.StopHost();
        manager.StopClient();
        lobbyID = string.Empty;
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        // Debug.LogError("OnGUI");
        //if the component is not initialized then dont continue
        //if (!EOSSDKComponent.Initialized)
        //{
        //    return;
        //}

        if (EOSSDKComponent.LocalUserProductId != null)
            playerID = EOSSDKComponent.LocalUserProductId.ToString();
        //start UI
        GUILayout.BeginHorizontal();

        //draw side buttons
        DrawMenuButtons();

        //draw scroll view
        GUILayout.BeginScrollView(Vector2.zero, GUILayout.MaxHeight(400));
        GUILayout.Label("State: " + connectionState);
        GUILayout.Label("FPS: " + FPS);
        GUILayout.Label("PlayerProductID: " + playerID);
        GUILayout.Label("LobbyID: " + lobbyID);
        GUILayout.Label(loadingtext);

        //runs when we want to show the lobby list
        if (_showLobbyList && !_showPlayerList)
        {
            DrawLobbyList();
        }
        //runs when we want to show the player list and we are connected to a lobby
        else if (!_showLobbyList && _showPlayerList && _eosLobby.ConnectedToLobby)
        {
            DrawLobbyMenu();
        }

        GUILayout.EndScrollView();

        GUILayout.EndHorizontal();
    }

    private void DrawMenuButtons()
    {
        //start button column
        GUILayout.BeginVertical();

        //decide if we should enable the create and find lobby buttons
        //prevents user from creating or searching for lobbies when in a lobby
        GUI.enabled = !_eosLobby.ConnectedToLobby;

        #region Draw Create Lobby Button

        GUILayout.BeginHorizontal();

        //create lobby button
        if (GUILayout.Button("Create Lobby"))
        {
            CreateLobbyy(lobbyName, maxPlayers, vis, latejoin);
        }

        vis = GUILayout.Toggle(vis, "Public");

        //maxPlayers = EditorGUILayout.IntField(maxPlayers, GUILayout.Width(50));

        lobbyName = GUILayout.TextField(lobbyName, 40, GUILayout.Width(200));

        GUILayout.EndHorizontal();

        #endregion

        //find lobby button
        if (GUILayout.Button("Find Lobbies"))
        {
            loadingtext = "Loading lobbies..";
            _eosLobby.FindLobbies();
        }


        //decide if we should enable the leave lobby button
        //only enabled when the user is connected to a lobby
        GUI.enabled = _eosLobby.ConnectedToLobby;

        if (GUILayout.Button("Leave Lobby"))
        {
            _eosLobby.LeaveLobby();
        }

        GUI.enabled = true;

        GUILayout.EndVertical();
    }

    private void DrawLobbyList()
    {
        //draw labels
        loadingtext = "Done";
        GUILayout.BeginHorizontal();
        GUILayout.Label("Lobby Name", GUILayout.Width(220));
        GUILayout.Label("Player Count");
        GUILayout.EndHorizontal();

        //draw lobbies
        foreach (LobbyDetails lobby in _foundLobbies)
        {
            //get lobby name
            Attribute? lobbyNameAttribute = new Attribute();
            LobbyDetailsCopyAttributeByKeyOptions copyOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = LobbyNameKey };
            lobby.CopyAttributeByKey(ref copyOptions, out lobbyNameAttribute);

            //draw the lobby result
            GUILayout.BeginHorizontal(GUILayout.Width(400), GUILayout.MaxWidth(400));

            if (lobbyNameAttribute.HasValue && lobbyNameAttribute.Value.Data.HasValue)
            {
                var data = lobbyNameAttribute.Value.Data.Value;
                //draw lobby name
                GUILayout.Label(data.Value.AsUtf8.Length > 30 ? data.Value.AsUtf8.ToString().Substring(0, 27).Trim() + "..." : data.Value.AsUtf8, GUILayout.Width(175));
                GUILayout.Space(75);
            }
            //draw player count
            LobbyDetailsGetMemberCountOptions memberCountOptions = new LobbyDetailsGetMemberCountOptions { };
            GUILayout.Label(lobby.GetMemberCount(ref memberCountOptions).ToString());
            GUILayout.Space(75);

            //draw join button
            if (GUILayout.Button("Join", GUILayout.ExpandWidth(false)))
            {
                _eosLobby.JoinLobby(lobby);
            }

            GUILayout.EndHorizontal();
        }
    }

    private void DrawLobbyMenu()
    {
        //draws the lobby name
        var lobbyNameAttribute = _lobbyData.Find((x) => x.Data.HasValue && x.Data.Value.Key == LobbyNameKey);
        if (!lobbyNameAttribute.Data.HasValue)
        {
            return;
        }
        GUILayout.Label("Name: " + lobbyNameAttribute.Data.Value.Value.AsUtf8);

        //draws players
        LobbyDetailsGetMemberCountOptions memberCountOptions = new LobbyDetailsGetMemberCountOptions();
        var playerCount = _eosLobby.ConnectedLobbyDetails.GetMemberCount(ref memberCountOptions);
        for (int i = 0; i < playerCount; i++)
        {
            GUILayout.Label("Player " + i);
        }
    }
#endif
}
