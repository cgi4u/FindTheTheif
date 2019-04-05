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

        public NPCPrefabSet NPCPrefabs;

        /// <summary>
        /// Set this thief's figure same as the randomly picked NPC prefab.
        /// Should be called as RPC to apply this change for all players.
        /// </summary>
        /// <param name="NPCIdx">randomly picked NPC's index in NPC prefab list.</param>
        [PunRPC]
        private void SetSpriteAndAnimation(int NPCIdx)
        {
            GetComponent<SpriteRenderer>().sprite = NPCPrefabs.Get(NPCIdx).GetComponent<SpriteRenderer>().sprite;
            GetComponent<Animator>().runtimeAnimatorController = NPCPrefabs.Get(NPCIdx).GetComponent<Animator>().runtimeAnimatorController;
        }

        private ItemController itemInHand;
        public ItemController StoleItem
        {
            get
            {
                return itemInHand;
            }
        }

        [PunRPC]
        public void StealItemInPoint(int itemPoint)
        {
            itemInHand = MapDataManager.Instance.ItemGenPoints[itemPoint].Item;
            itemInHand.Stolen();
        }

        [PunRPC]
        public void PutItemInPoint()
        {
            itemInHand = null;
        }

        /// <summary>
        /// Excuted when this thief player is arrested by a detective.
        /// </summary>
        [PunRPC]
        public void Arrested()
        {
            if (itemInHand != null)
                itemInHand.Restored();

            if (photonView.isMine)
                PhotonNetwork.Destroy(this.gameObject);
        }
    }
}
