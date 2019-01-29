using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class ItemGenPoint : Photon.PunBehaviour
    {
        [SerializeField]
        private bool isItemExist;
        [SerializeField]
        private ItemController item;
        public ItemController Item
        {
            get
            {
                return item;
            }
        }

        public void SetItem(ItemController _item)
        {
            item = _item;
            isItemExist = true;
        }

        public void StealItem()
        {
            Debug.Log("Steal Item");
            isItemExist = false;
        }

        public void ItemReturned()
        {
            Debug.Log("Item Returned");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isItemExist)
                return;

            //Only works when local player walks in front of this point
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player == PlayerController.LocalPlayer)
            {
                Debug.Log("Player On");
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!isItemExist)
                return;

            //Only works when local player walks out front of this point
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player == PlayerController.LocalPlayer)
            {
                Debug.Log("Player Off");
            }
        }
    }
}