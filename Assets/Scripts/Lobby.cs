using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Lobby : Photon.PunBehaviour
{
    #region Public Properties

    [Tooltip("The maximum number of players per room. When a room is full, it can't be joined by new players, and so new room will be created")]
    public byte MaxPlayersPerRoom = 5;

    public InputField nameInputField;

    #endregion


    #region Unity Callbacks

    // Use this for initialization
    private void Awake()
    {
        
    }

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
        PhotonNetwork.CreateRoom(null, new RoomOptions() { MaxPlayers = MaxPlayersPerRoom }, null);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        Debug.Log("PlayerName: " + PhotonNetwork.playerName);
        Debug.Log("Maximum Player:" + PhotonNetwork.room.MaxPlayers);

        // #Critical: We only load if we are the first player, 
        //     else we rely on  PhotonNetwork.automaticallySyncScene to sync our instance scene.
        // 즉, 룸에 참가했을 때 플레이어 수가 1명일 때, 한 마디로 나만 있을때만 레벨을 로드한다는 것이고
        //  그게 아니라면 자동 싱크를 통해 룸을 로드해야 한다는 것.
        if (PhotonNetwork.room.PlayerCount == 1)
        {
            Debug.Log("We load the 'Demo Room' ");

            // 룸을 로드. 동기화를 위해 LoadLevel 사용.
            PhotonNetwork.LoadLevel("Demo Room");
        }
    }

    /*public override void OnReceivedRoomListUpdate()
    {
        roomList.text = "";

        RoomInfo[] roomInfo = PhotonNetwork.GetRoomList();
        for (int i = 0; i < roomInfo.Length; i++)
        {
            Debug.Log(roomInfo[i].Name);
            roomList.text += roomInfo[i].Name + "\n";
        }
    }*/

    #endregion


    #region Legacy
    /*
    public void OpenRoomPanel()
    {
        roomCreatePanel.SetActive(true);
    }

    string roomName;
    int maxMember;
    public void CreateRoom()
    {
        if (roomNameInputField.text.Length == 0)
        {
            Debug.Log("Error: Room name field is empty.");
            return;
        }

        if (maxMemberInputField.text.Length == 0)
        {
            Debug.Log("Error: The maximum number of members is empty.");
            return;
        }

        roomName = roomNameInputField.text;
        int.TryParse(maxMemberInputField.text, out maxMember);
        PhotonNetwork.CreateRoom(roomName, new RoomOptions() { MaxPlayers = (byte)maxMember }, null);
    }

    public void CancelCreate()
    {
        roomCreatePanel.SetActive(false);
    }

    private char NumberValidate(char addedChar)
    {
        if (addedChar < '0' || addedChar > '9')
        {
            addedChar = '\0';
        }

        return addedChar;
    }
    */
    #endregion

}
