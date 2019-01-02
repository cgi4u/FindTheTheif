using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheTheif
{
    public class Lobby : Photon.PunBehaviour
    {
        #region Public Properties

        /* 이 둘은 현 시점에서 고정값이지만, 유저가 변경할 수 있도록 수정될 가능성이 있으니 여기서 결정하고 룸으로 전달하는 식으로 한다. */
        //The number of players per room 
        public byte playersPerRoom;
        //The number players in each team
        public byte theifPerRoom;

        public InputField nameInputField;

        //UI for game match wait screen
        public GameObject curPlayerNumPanel;
        public Text curPlayerNum;

        #endregion


        #region Unity Callbacks

        void Start()
        {


            /*
            maxMemberInputField.onValidateInput 
                += delegate (string input, int charIndex, char addedChar) {
                    //Debug.Log(input.Length);
                    return NumberValidate(addedChar);
                };

            RoomInfo[] roomInfo = PhotonNetwork.GetRoomList();
            for (int i = 0; i < roomInfo.Length; i++)
            {
                roomList.text += roomInfo[i].Name + "\n";
            }
            */
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitLobby();
            }
        }

        #endregion


        #region Public Mehods

        public void GameMatching()
        {
            if (string.IsNullOrEmpty(nameInputField.text))
            {
                Debug.Log("Please write your player name.");
                return;
            }

            PhotonNetwork.playerName = nameInputField.text;

            PhotonNetwork.JoinRandomRoom();
        }

        public void ExitLobby()
        {
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.Disconnect();
            SceneManager.LoadScene("Launcher");
        }

        #endregion


        #region Private Methods

        #endregion


        #region Photon Callbacks

        //랜덤 방참가(JoinRandomRoom)가 실패했을 때 콜백
        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg)
        {
            Debug.Log("DemoAnimator/Launcher:OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one.");
            // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
            PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = playersPerRoom }, null);
            curPlayerNumPanel.SetActive(true);
        }

        public override void OnJoinedRoom()
        {
            Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
            Debug.Log("PlayerName: " + PhotonNetwork.playerName);
            Debug.Log("Maximum Player:" + PhotonNetwork.room.MaxPlayers);

            curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();

            //Codes for Test
            /*Hashtable cp = new Hashtable();
            cp["Team"] = Team.detective;
            PhotonNetwork.player.SetCustomProperties(cp);
            PhotonNetwork.LoadLevel("Demo Room");*/
        }

        public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
        {
            curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();

            // Load scene when the local player is the master client
            if (PhotonNetwork.room.PlayerCount == playersPerRoom
                && PhotonNetwork.player.ID == PhotonNetwork.masterClient.ID)
            {
                //Choose theif players randomly
                PhotonPlayer[] users = PhotonNetwork.playerList;
                int theifCount = theifPerRoom;
                bool[] isPlayerTheif = new bool[playersPerRoom];
                while (theifCount != 0)
                {
                    int rp = Random.Range(0, playersPerRoom - 1);

                    if (isPlayerTheif[rp] == false)
                    {
                        isPlayerTheif[rp] = true;
                        theifCount--;
                    }
                }

                //Save players' team in their custom property
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    Hashtable playerCp = new Hashtable();

                    if (isPlayerTheif[player.ID - 1] == true)
                    {
                        playerCp["Team"] = Team.theif;
                    }
                    else
                    {
                        playerCp["Team"] = Team.detective;
                    }
                    player.SetCustomProperties(playerCp);
                }

                //Set room properties
                //1. The Number of players in the room, 2. The number of thiefs in the room
                // ISSUE:  플레이어 수를 건네줘야 할 필요가 있는가? PhotonNetWork에서 참조 가능하고 이게 더 정확할 수 있음
                Hashtable roomCp = new Hashtable();
                roomCp["Player Number"] = playersPerRoom;
                roomCp["Theif Number"] = theifPerRoom;
                PhotonNetwork.room.SetCustomProperties(roomCp);

                Debug.Log("We load the 'Demo Room' ");
                //Load the game level. Use LoadLevel to synchronize(automaticallySyncScene is true)
                PhotonNetwork.LoadLevel("Demo Room");
            }
        }

        #endregion

    }
}
