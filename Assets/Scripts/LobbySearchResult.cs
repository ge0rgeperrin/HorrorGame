using PallonAnticheat;

namespace Subterranea
{
    using System;
    using Epic.OnlineServices;
    using Epic.OnlineServices.Lobby;
    using EpicTransport;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public class LobbySearchResult : MonoBehaviour
    {
        public TMP_Text LobbyName;
        public TMP_Text PlayersInLobby;
        public TMP_Text LobbyRegion;
        public Button JoinLobbyButton;

        private LobbyDetails thisLobbyDetails;
        private LobbyDetailsInfo? thisLobbyDetailsInfo;
        private EOSLobby eosLobby => EOSSDKComponent.Instance.eosLobby;

        public string GetPlayersInLobby(uint slots)
        {
            int freeSlots = Convert.ToInt32(slots);
            int playersInLobby = 6 - freeSlots;
            return $"{playersInLobby}/6";
        }

        public void LoadLobbyInfo(LobbyDetails lobbyDetails)
        {
            JoinLobbyButton.interactable = false;
            thisLobbyDetails = lobbyDetails;
        
            LobbyDetailsCopyInfoOptions lobbyInfoOptions = new LobbyDetailsCopyInfoOptions {};
            Result copyLobbyDetails = lobbyDetails.CopyInfo(ref lobbyInfoOptions, out LobbyDetailsInfo? lobbyDetailsInfo);
            thisLobbyDetailsInfo = lobbyDetailsInfo;

            var getRegionAttributeOptions = new LobbyDetailsCopyAttributeByKeyOptions { AttrKey = "region" };
            Result getRegion = lobbyDetails.CopyAttributeByKey(ref getRegionAttributeOptions, out Epic.OnlineServices.Lobby.Attribute? regionAttribute);
            
            if (copyLobbyDetails == Result.Success && lobbyDetailsInfo.HasValue)
            {
                MonkeLogger.Log("Copied lobby details from EOS successfully.");

                var region = regionAttribute.Value.Data.Value.Value.ToString();
                var lobbyName = lobbyDetailsInfo.Value.LobbyId;
                LobbyName.text = lobbyName;
                JoinLobbyButton.interactable = true;
                PlayersInLobby.text = GetPlayersInLobby(lobbyDetailsInfo.Value.AvailableSlots);
                
                if (getRegion == Result.Success)
                {
                    LobbyRegion.text = region;
                }
                else
                {
                    MonkeLogger.Log(MonkeLogger.LogLevel.Error,$"Failed to get region of lobby {lobbyName} from EOS. Error: {getRegion}");
                }
            }
            else
            {
                MonkeLogger.Log(MonkeLogger.LogLevel.Error,$"Failed to copy lobby details from EOS. Error: {copyLobbyDetails}");
                Destroy(gameObject);
            }
        }

        public void JoinLobby()
        {
            MonkeLogger.Log($"Joining lobby found from search result... Host: {thisLobbyDetailsInfo.Value.LobbyOwnerUserId} ID: {thisLobbyDetailsInfo.Value.LobbyId}");
            eosLobby.JoinLobby(thisLobbyDetails);
        }
    }
}