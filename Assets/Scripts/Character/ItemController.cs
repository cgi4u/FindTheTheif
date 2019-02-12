using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class ItemController : Photon.PunBehaviour
    {
        public readonly int ItemAttrNum = 3;
        public readonly int ItemAttrTypeNum = 3;

        // Transparent sprite used when this item is stolen.
        [SerializeField]
        protected Sprite transparentSprite;

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

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(this);
        }

        [PunRPC]
        public void Init(int itemGenPointIdx)
        {
            genPoint = MapDataManager.Instance.ItemGenPoints[itemGenPointIdx];
            MapDataManager.Instance.ItemGenPoints[itemGenPointIdx].SetItem(this);
        }

        private void OnBecameVisible()
        {
            Debug.Log(gameObject.name + "is visible.");
            UIManager.Instance.AddCheckedItem(this);
        }
    }
}

