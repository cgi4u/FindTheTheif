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

        //private 변경 필요
        public int floorNum;
        public int roomNum;

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(myColor, myAge, myUsage, transform.position);
        }

        public void Init(int _floorNum, int _roomNum)
        {
            floorNum = _floorNum;
            roomNum = _roomNum;
        }

        private void OnBecameVisible()
        {
            Debug.Log(gameObject.name + "is visible.");
            UIManager.Instance.AddCheckedItem(this);
        }
    }
}

