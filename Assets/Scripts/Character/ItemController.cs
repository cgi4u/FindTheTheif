using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class ItemController : MonoBehaviour
    {
        public readonly int ItemAttrNum = 3;
        public readonly int ItemAttrTypeNum = 3;

        // This item's attributes.
        public ItemColor myColor;
        public ItemAge myAge;
        public ItemUsage myUsage;

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

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(myColor, myAge, myUsage, transform.position);
        }

        [PunRPC]
        public void Init(int _floorNum, int _roomNum, int genPointIdx, int[] itemNumInGenPoint, int[] targetItemPointIndex)
        {
            floorNum = _floorNum;
            roomNum = _roomNum;

            if gen
        }

        private void OnBecameVisible()
        {
            Debug.Log(gameObject.name + "is visible.");
            UIManager.Instance.AddCheckedItem(this);
        }
    }
}

