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

        private bool sensingMode = false;

        private void Awake()
        {
            if (!photonView.isMine)
                return;

            if (localThief != null)
            {
                Debug.LogError("Multiple instantiation of the local thief player.");
                PhotonExtends.Destroy(gameObject);
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
        /// <summary>
        /// The item stolen by this thief. Null when this thief doesn't have any item.
        /// </summary>
        public ItemController ItemInHand
        {
            get
            {
                return itemInHand;
            }
        }

        GameObject sensingAlert = null;

        [PunRPC]
        public void StealItemInPoint(int itemPoint)
        {
            itemInHand = MapDataManager.Instance.ItemGenPoints[itemPoint].Item;
            if (itemInHand.IsTrapped)
                sensingAlert = UIManager.Instance.SetAlertAndReturnObj(GetComponent<PlayerController>());

            itemInHand.Stolen();
        }

        [PunRPC]
        public void PutItemInPoint()
        {
            itemInHand = null;
            CheckAndDestroyAlert();
        }

        private void CheckAndDestroyAlert()
        {
            if (sensingAlert != null)
                Destroy(sensingAlert);
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

        private void OnDestroy()
        {
            CheckAndDestroyAlert();
        }

        #region Skill Implementation

        public GameObject secretPath;
        private GameObject unlinkedPath = null;

        public bool MakeSecretPath()
        {
            if (!CheckCurPos()
                || Mathf.Floor(transform.position.x) != transform.position.x 
                || Mathf.Floor(transform.position.y) != transform.position.y)
            {
                Debug.LogError("Secret path cannot be generated in current site.");
                return false;
            }

            GameObject newPath = Instantiate(secretPath, transform.position, Quaternion.identity);
            if (unlinkedPath != null)
            {
                unlinkedPath.GetComponent<SecretPathController>().Link(newPath.GetComponent<SecretPathController>());
                newPath.GetComponent<SecretPathController>().Link(unlinkedPath.GetComponent<SecretPathController>());

                unlinkedPath = null;
            }
            else
                unlinkedPath = newPath;

            return true;
        }

        private bool CheckCurPos()
        {
            RaycastHit2D[] cols = Physics2D.BoxCastAll(transform.position, new Vector2(1f, 1f), 0, new Vector2(0f, 0f)); 
            for (int i = 0; i < cols.Length; i++)
            {
                if (cols[i].collider.GetComponent<SecretPathController>() != null)
                    return false;
            }

            return true;
        }

        public GameObject smokePrefab;

        public bool MakeSmoke()
        {
            GameObject newSmoke = PhotonNetwork.Instantiate(smokePrefab.name, 
                new Vector2(Mathf.Floor(transform.position.x), Mathf.Floor(transform.position.y)), Quaternion.identity, 0);
            newSmoke.GetComponent<SmokeController>().Init(15f);

            return true;
        }

        #endregion


    }
}
