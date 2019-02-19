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
            ThiefController.LocalThief.GetComponent<PhotonView>().
                RPC("StealItemInPoint", PhotonTargets.AllViaServer, CurGenPoint.Index);
            CurGenPoint.Item.GetComponent<PhotonView>().RPC("Stolen", PhotonTargets.AllViaServer);

            gameObject.SetActive(false);
        }
    }
}
