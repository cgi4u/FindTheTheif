using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class StealPopUp : MonoBehaviour
    {
        public ItemGenPoint CurGenPoint { get; set; }
            
        public void OnStealButton()
        {
            ThiefController.LocalThief.photonView.
                RPC("StealItemInPoint", PhotonTargets.AllViaServer, CurGenPoint.Index);
            PutItemPoint.SetPointPopup(true);

            gameObject.SetActive(false);
        }
    }
}
