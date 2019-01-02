using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class ChatSyncPort : Photon.PunBehaviour, IPunObservable
    {
        #region Private Properties

        ChatController chatController;

        bool isMyTeam = true;

        #endregion

        #region Unity Callbacks

        void Awake()
        {
            /*if (!photonView.isMine)
                isMyTeam = false;
                //Destroy(this.gameObject);*/
        }

        void Update()
        {
            if (chatController == null && ChatController.chatController != null)
                chatController = ChatController.chatController;
        }

        #endregion


        #region Photon Serialization

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            string msg;

            if (stream.isWriting)
            {
                if (chatController != null)
                {
                    msg = chatController.GetMsgToSend();
                    if (msg != null)
                    {
                        stream.SendNext(msg);
                    }
                }
            }
            else if (isMyTeam)
            {
                msg = (string)stream.ReceiveNext();
                if (msg != null)
                    chatController.AddChatMsg(msg);
            }
        }

        #endregion
    }
}
