using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour {
    #region Public Properties

    //The prefab of a chat port
    public GameObject chatPortPrefab;
    
    //The singleton of the chat system controller
    public static ChatController chatController;

    //UI components for chat
    public InputField chatInputField;
    public Text chatText;

    #endregion


    #region Unity Callbacks
    
    void Awake()
    {
        chatController = this;
    }

    void Start()
    {
        PhotonNetwork.Instantiate(chatPortPrefab.name, transform.position, Quaternion.identity, 0);
    }

    #endregion


    #region Private Properties

    List<string> msgsToSend = new List<string>();
    List<string> chatMsgs = new List<string>();

    #endregion


    #region Public Methods

    public void OnSendButtonClicked()
    {
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            string msg = PhotonNetwork.playerName + ": " + chatInputField.text;
            msgsToSend.Add(msg);

            AddChatMsg(msg);
        }
    }

    public string GetMsgToSend()
    {
        string msg = null;

        if (msgsToSend.Count != 0)
        {
            msg = msgsToSend[0];
            msgsToSend.RemoveAt(0);
        }

        return msg;
    }

    public void AddChatMsg(string msg)
    {
        chatMsgs.Add(msg);
        checkNumOfMsgs();
        chatText.text = msg;
    }

    #endregion


    #region Private Methods

    void checkNumOfMsgs()
    {
        if (chatMsgs.Count > 10)
            chatMsgs.RemoveAt(0);
    }

    #endregion

}
