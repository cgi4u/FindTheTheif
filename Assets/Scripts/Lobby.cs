using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Lobby : Photon.PunBehaviour
{
    #region Public Properties

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte maxPlayersPerRoom;

    public InputField nameInputField;

    //UI for game match wait screen
    public GameObject curPlayerNumPanel;
    public Text curPlayerNum;

    #endregion


    #region Unity Callbacks

    void Start () {
        

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
    void Update () {
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
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = maxPlayersPerRoom }, null);
        curPlayerNumPanel.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        Debug.Log("PlayerName: " + PhotonNetwork.playerName);
        Debug.Log("Maximum Player:" + PhotonNetwork.room.MaxPlayers);

        curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();
        //PhotonNetwork.LoadLevel("Demo Room");
    }

    public override void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
    {
        curPlayerNum.text = PhotonNetwork.room.PlayerCount.ToString();

        // Load scene when the local player is the master client
        if (PhotonNetwork.room.PlayerCount == maxPlayersPerRoom
            && PhotonNetwork.player.ID == PhotonNetwork.masterClient.ID)
        {

            /*
            string[] users = PhotonNetwork.room.ExpectedUsers;  //들어올것이라 예상되는 플레이어, 즉 특정 플레이어가 들어올 자리를 미리 비워놓는것
            foreach (string user in users)
                Debug.Log(user);
            */

            PhotonPlayer[] users = PhotonNetwork.playerList;
            foreach (PhotonPlayer user in users)
            {
                Hashtable cp = new Hashtable();
                cp["Test"] = "Text";
                user.SetCustomProperties(cp);
                Debug.Log(user.ID);
            }
            

            Debug.Log("We load the 'Demo Room' ");
            //Load the game level. Use LoadLevel to synchronize(automaticallySyncScene is true)
            PhotonNetwork.LoadLevel("Demo Room");
        }
    }

    #endregion

}
