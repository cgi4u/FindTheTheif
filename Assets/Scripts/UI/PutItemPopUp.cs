using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class PutItemPopUp : MonoBehaviour
    {
        /// <summary>
        /// Put current stolen item to this point.
        /// </summary>
        public void OnPutItemButton()
        {
            ThiefController localThief = ThiefController.LocalThief;

            MultiplayRoomManager.Instance.photonView
                .RPC("StealSuccess", PhotonTargets.AllViaServer, PhotonNetwork.player.ID, localThief.ItemInHand.GenPoint.Index);
            localThief.photonView.RPC("PutItemInPoint", PhotonTargets.All);
            PutItemPoint.SetPointPopup(false);

            gameObject.SetActive(false);
        }
    }
}
