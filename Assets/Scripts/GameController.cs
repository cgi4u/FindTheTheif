using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class GameController: MonoBehaviour {
    #region Public Properties

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;

    #endregion


    #region Unity Callbacks

    // Use this for initialization
    void Start () {
        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Room Controller'", this);
        }
        else
        {
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(.5f, .5f, 0f), Quaternion.identity, 0);

            /*
            //Instantiate Chat Controller for each user
            GameObject localChatInstantce = PhotonNetwork.Instantiate(chatPrefab.name, new Vector3(0f, 0f, 10f), Quaternion.identity, 0);

            //Connect the local chat controller with UI components(button, chat screen, input field)
            ChatController localChatController = localChatInstantce.GetComponent<ChatController>();
            localChatController.chatInputField = chatInputField;
            localChatController.chatText = chatText;
            chatSendButton.onClick.AddListener(localChatController.OnSendButtonClicked);*/
        }
    }
	
	// Update is called once per frame
	void Update () {
        
    }

    #endregion


    #region Public Methods

    public void ExitRoom()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    #endregion
}
