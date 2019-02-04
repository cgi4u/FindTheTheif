using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class ItemController : Photon.PunBehaviour
    {
        public readonly int ItemAttrNum = 3;
        public readonly int ItemAttrTypeNum = 3;

        // Transparent sprite used when this item is stolen.
        [SerializeField]
        protected Sprite transparentSprite;

        // This item's attributes.
        public ItemColor myColor;
        public ItemAge myAge;
        public ItemUsage myUsage;

        #region Properties after generation in game

        [SerializeField]
        private int floorNum;
        public int FloorNum
        {
            get
            {
                return floorNum;
            }
        }
        [SerializeField]
        private int roomNum;
        public int RoomNum
        {
            get
            {
                return roomNum;
            }
        }

        /*
        [SerializeField]
        private bool isStolen = false;
        [SerializeField]
        private bool isTarget = false;
        */

        #endregion

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(myColor, myAge, myUsage, transform.position);
        }

        [PunRPC]
        public void Init(int _floorNum, int _roomNum, int itemGenPoint)
        {
            floorNum = _floorNum;
            roomNum = _roomNum;

            MapDataManager.Instance.ItemGenPoints[itemGenPoint].SetItem(this);
        }

        private void OnBecameVisible()
        {
            Debug.Log(gameObject.name + "is visible.");
            UIManager.Instance.AddCheckedItem(this);
        }
    }
}

