using System.Collections;
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
        private ItemController item;
        public ItemController Item
        {
            get
            {
                return item;
            }
        }

        [SerializeField]
        private Vector2 itemOffset;
        public Vector2 ItemPos
        {
            get
            {
                return (Vector2)transform.position + itemOffset;
            }
        }

        /// <summary>
        /// Index of this point in the item generation point list of MapDataManager.
        /// </summary>
        public int Index { get; set; }

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

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        public void SetItem(ItemController _item)
        {
            item = _item;
        }

        public void ItemReturned()
        {
            Debug.Log("Item Returned");
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (item == null || item.IsStolen)
                return;

            //Only works when local thief player walks in front of this point
            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem == null)
            {
                UIManager.Instance.SetStealPopUp(this);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (item == null || item.IsStolen)
                return;

            //Only works when local player walks out front of this point
            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem == null)
            {
                UIManager.Instance.RemoveStealPopUp();
            }
        }
    }
}