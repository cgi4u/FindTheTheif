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

        [PunRPC]
        private void SetSpriteAndAnimation(int NPCIdx)
        {
            GetComponent<SpriteRenderer>().sprite = MultiplayRoomManager.Instance.NPCPrefabs[NPCIdx].GetComponent<SpriteRenderer>().sprite;
            GetComponent<Animator>().runtimeAnimatorController = MultiplayRoomManager.Instance.NPCPrefabs[NPCIdx].GetComponent<Animator>().runtimeAnimatorController;
        }

        private ItemController stoleItem;
        public ItemController StoleItem
        {
            get
            {
                return stoleItem;
            }
        }

        [PunRPC]
        public void StealItemInPoint(int itemPoint)
        {
            stoleItem = MapDataManager.Instance.ItemGenPoints[itemPoint].Item;
        }

        [PunRPC]
        public void PutItemInPoint()
        {
            stoleItem = null;
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
