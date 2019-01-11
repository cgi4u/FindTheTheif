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

        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(myColor, myAge, myUsage, transform.position);
        }
    }
}

