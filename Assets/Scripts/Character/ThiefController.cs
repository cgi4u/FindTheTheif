using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(PlayerController))]
    public class ThiefController : Photon.PunBehaviour
    {
        private static ThiefController localThief;
        public static ThiefController LocalThief
        {
            get
            {
                return localThief;
            }
        }

        private void Awake()
        {
            if (!photonView.isMine)
                return;

            if (localThief != null)
            {
                Debug.LogError("Multiple instantiation of the local thief player.");
                return;
            }

            localThief = this;
        }

        private ItemController stoleItem = null;
        private bool hasItem = false;
        public bool HasItem
        {
            get
            {
                return hasItem;
            }
        }
        [PunRPC]
        public void StealItemInPoint(int itemPoint)
        {
            stoleItem = MapDataManager.Instance.ItemGenPoints[itemPoint].Item;
            hasItem = true;
        }

        public override void OnOwnershipTransfered(object[] viewAndPlayers)
        {
            if (stoleItem != null)
            {
                stoleItem.GetComponent<PhotonView>().RPC("Restored", PhotonTargets.AllViaServer);
            }

            if (photonView.isMine)
                PhotonNetwork.Destroy(gameObject);
        }
    }
}
