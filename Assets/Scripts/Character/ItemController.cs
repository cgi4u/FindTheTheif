using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class ItemController : Photon.PunBehaviour
    {
        public readonly int ItemAttrNum = 3;
        public readonly int ItemAttrTypeNum = 3;

        /// <summary>
        /// The list  of items that the local player discovered in game.
        /// </summary>
        static List<ItemController> discoveredItems;

        /// <summary>
        /// The list of items stolen(not just carried by theif, but put into the destination point.
        /// </summary>
        static List<ItemController> stolenItems;

        [SerializeField]
        private Sprite orgSprite;
        [SerializeField]
        protected Sprite transparentSprite;
        public bool IsStolen { get; set; } = false;
        public bool IsStolenChecked { get; set; } = false;

        #region Intrinsic Item Properties

        [SerializeField]
        private ItemColor myColor;
        public ItemColor Color
        {
            get
            {
                return myColor;
            }
        }

        [SerializeField]
        private ItemAge myAge;
        public ItemAge Age
        {
            get
            {
                return myAge;
            }
        }

        [SerializeField]
        private ItemUsage myUsage;
        public ItemUsage Usage
        {
            get
            {
                return myUsage;
            }
        }

        #endregion

        #region Properties After Generation

        private ItemGenPoint genPoint;
        /// <summary>
        /// The item generation point in which this item is generated.
        /// </summary>
        public ItemGenPoint GenPoint
        {
            get
            {
                return genPoint;
            }
        }

        #endregion

        /// <summary>
        /// Reset discovered / stolen item list for each game.
        /// </summary>
        public static void ResetItemLists()
        {
            discoveredItems = new List<ItemController>();
            stolenItems = new List<ItemController>();
        }

        private void Awake()
        {
            transform.parent = MultiplayRoomManager.Instance.SceneObjParent;

            orgSprite = GetComponent<SpriteRenderer>().sprite;
        }

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        [PunRPC]
        public void Init(int itemGenPointIdx)
        {
            genPoint = MapDataManager.Instance.ItemGenPoints[itemGenPointIdx];
            MapDataManager.Instance.ItemGenPoints[itemGenPointIdx].SetItem(this);
        }

        [PunRPC]
        public void Stolen()
        {
            IsStolen = true;

            // Change sprite to the transparent.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = transparentSprite;
            if (spriteRenderer.isVisible)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;
            }
        }

        [PunRPC]
        public void Restored()
        {
            IsStolen = false;

            // Change sprite to the item's original one.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = orgSprite;
            if (spriteRenderer.isVisible)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;
            }
        }

        private void OnBecameVisible()
        {
            if (!discoveredItems.Contains(this))
            {
                if (!IsStolen)
                {
                    Debug.Log(gameObject.name + "is visible.");
                    discoveredItems.Add(this);
                }
            }
            else
            {
                if (IsStolen)
                {
                    IsStolenChecked = true;
                }
                else
                {
                    IsStolenChecked = false;
                }
            }

            UIManager.Instance.RenewCheckedList(discoveredItems);
        }

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(this, Input.mousePosition);
        }
    }
}

