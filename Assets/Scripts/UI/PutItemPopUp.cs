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

            MultiplayRoomManager.Instance.GetComponent<PhotonView>()
                .RPC("ItemStolen", PhotonTargets.AllViaServer, localThief.StoleItem.GenPoint.Index);
            localThief.GetComponent<PhotonView>().RPC("PutItemInPoint", PhotonTargets.All);

            gameObject.SetActive(false);
        }
    }
}
