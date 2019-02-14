﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class ItemGenPoint : MonoBehaviour
    {
        private ExhibitRoom room;
        public ExhibitRoom Room
        {
            get
            {
                return room;
            }
        }

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

        private void Awake()
        {
            room = GetComponentInParent<ExhibitRoom>();
            if (room == null)
            {
                Debug.LogError("Error in ItemGenPoint " + gameObject.GetInstanceID());
                Debug.LogError("An Item generation point should be a child of an exhibit room.");
                return;
            }
        }

        public void SetItem(ItemController _item)
        {
            item = _item;
            isItemExist = true;
        }

        public void ItemReturned()
        {
            Debug.Log("Item Returned");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isItemExist || MultiplayRoomManager.Instance.MyTeam != Team.Thief)
                return;

            //Only works when local player walks in front of this point
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player == PlayerController.LocalPlayer)
            {
                UIManager.Instance.SetStealPopUp(this);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!isItemExist || MultiplayRoomManager.Instance.MyTeam != Team.Thief)
                return;

            //Only works when local player walks out front of this point
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null && player == PlayerController.LocalPlayer)
            {
                UIManager.Instance.RemoveStealPopUp();
            }
        }
    }
}