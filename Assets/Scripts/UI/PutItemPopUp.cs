using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class PutItemPopUp : MonoBehaviour
    {
        /// <summary>
        /// Put item to the point
        /// </summary>
        public void OnPutItemButton()
        {
            ThiefController localThief = ThiefController.LocalThief;

            MultiplayRoomManager.Instance.photonView
                .RPC("StealSuccess", PhotonTargets.AllViaServer, PhotonNetwork.player.ID, localThief.StoleItem.GenPoint.Index);
            localThief.photonView.RPC("PutItemInPoint", PhotonTargets.All);

            gameObject.SetActive(false);
        }
    }
}
