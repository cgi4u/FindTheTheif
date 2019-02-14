using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class StealPopUp : MonoBehaviour
    {
        public ItemGenPoint CurGenPoint { get; set; }
            
        public void Steal()
        {
            Debug.Log(CurGenPoint.Item.name + " 훔치자!");
        }
    }
}
