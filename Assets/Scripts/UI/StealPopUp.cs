using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class StealPopUp : MonoBehaviour
    {
        public ItemGenPoint CurGenPoint { get; set; }
        
        /// <summary>
        /// Called when the item steal button is pushed. Set item to thief player and activate put point popup.
        /// </summary>
        public void OnStealButton()
        {
            ThiefController.LocalThief.photonView.
                RPC("StealItemInPoint", PhotonTargets.AllViaServer, CurGenPoint.Index);
            //MultiplayRoomManager.Instance.photonView.RPC("SetStealAlert", PhotonTargets.All, CurGenPoint.Index);
            PutItemPoint.SetPointPopup(true);

            gameObject.SetActive(false);
        }
    }
}
