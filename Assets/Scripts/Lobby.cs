using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheThief
{
    public class Lobby : Photon.PunBehaviour
    {
        /* 이 둘은 현 시점에서 고정값이지만, 유저가 변경할 수 있도록 수정될 가능성이 있으니 여기서 결정하고 룸으로 전달하는 식으로 한다. */
        //The number of players per room 
        public byte playersPerRoom;
        //The number players in each team
        public byte thievesPerRoom;

        public InputField nameInputField;

        //UI for game match wait screen
        public GameObject curPlayerNumPanel;
        public Text curPlayerNum;
        public CanvasGroup interactableUIGroup;
        public GameObject grayPanel;

        public GameObject multiplayRoomManager;
        

        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitLobby();
            }
        }

        public void GameMatching()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                Debug.Log("Please write your player name.");
                return;
            }

            interactableUIGroup.interactable = false;
            grayPanel.SetActive(true);

            PhotonNetwork.playerName = nameInputField.text;
            PhotonNetwork.JoinRandomRoom();
        }

        public void ExitLobby()
        {
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Launcher");
        }

        //랜덤 방참가(JoinRandomRoom)가 실패했을 때 콜백
        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.");
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = playersPerRoom }, null);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
            Debug.Log("PlayerName: " + PhotonNetwork.playerName);
            Debug.Log("Maximum Player:" + PhotonNetwork.room.MaxPlayers);

            if (PhotonNetwork.room.PlayerCount == 1)    //When local player made this room
            {
                //Set room properties
                Hashtable roomCp = new Hashtable();
                //roomCp["Player Number"] = playersPerRoom;
                roomCp["Thieves Number"] = thievesPerRoom;
                PhotonNetwork.room.SetCustomProperties(roomCp);

                //InitializeAndLoadScene();
            }

            curPlayerNumPanel.SetActive(true);
            curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();

            //Codes for test with 1 user
            /*Hashtable cp = new Hashtable();
            cp["Team"] = Team.detective;
            PhotonNetwork.player.SetCustomProperties(cp);
            PhotonNetwork.LoadLevel("Demo Room");*/
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();

            // Load scene when the local player is the master client
            if (PhotonNetwork.room.PlayerCount == playersPerRoom && PhotonNetwork.isMasterClient)
            {
                PhotonNetwork.room.IsOpen = false;
                PhotonNetwork.InstantiateSceneObject(multiplayRoomManager.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0, null);
            }
        }

        public void WaitCancel()
        {
            PhotonNetwork.LeaveRoom();
            curPlayerNumPanel.SetActive(false);
            grayPanel.SetActive(false);
            interactableUIGroup.interactable = true;
        }
    }
}
