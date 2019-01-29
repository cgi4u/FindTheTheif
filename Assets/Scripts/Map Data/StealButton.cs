using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace com.MJT.FindTheTheif
{
    public class StealButton : MonoBehaviour, IPointerUpHandler
    {
        [SerializeField]
        private ItemGenPoint genPoint;

        public void OnPointerUp(PointerEventData eventData)
        {
            genPoint.StealItem();
        }
    }
}