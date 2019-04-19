using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    using static ItemProperties;

    public class ItemController : Photon.PunBehaviour
    {
        static List<ItemController> instances = new List<ItemController>();
        /// <summary>
        /// The list  of items that the local player discovered in game.
        /// </summary>
        static List<ItemController> discoveredItems = new List<ItemController>();

        [SerializeField]
        private Sprite orgSprite;
        [SerializeField]
        protected Sprite stolenSprite;

        [SerializeField]
        private Vector2 touchAreaSize;
        private Rect touchArea;

        public bool IsStolen { get; set; } = false;
        public bool IsStolenChecked { get; set; } = false;

        #region Intrinsic Item Properties

        [SerializeField]
        private EItemColor myColor;
        public EItemColor Color
        {
            get
            {
                return myColor;
            }
        }

        [SerializeField]
        private EItemAge myAge;
        public EItemAge Age
        {
            get
            {
                return myAge;
            }
        }

        [SerializeField]
        private EItemUsage myUsage;
        public EItemUsage Usage
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

        /// <summary>
        /// Prioirty in the discovered item list. Be changed when the stolen item has a same item property with this item.
        /// </summary>
        private int prioirty = 0;
        private int rank = 1;
        public int Rank
        {
            get
            {
                return rank;
            }
        }

        #endregion

        /// <summary>
        /// Reset discovered / stolen item list for each game.
        /// </summary>
        public static void ResetItemList()
        {
            instances.Clear();
            discoveredItems.Clear();
        }

        /// <summary>
        /// Renew the print prioirty of the discoverd item list(when an item is stolen)
        /// </summary>
        /// <param name="stolen"></param>
        public static void RenewDiscoveredItemPrioirty(ItemController stolen)
        {
            foreach (ItemController item in instances)
            {
                if (item.myColor == stolen.myColor)
                    item.prioirty += 1;
                if (item.myAge == stolen.myAge)
                    item.prioirty += 1;
                if (item.myUsage == stolen.myUsage)
                    item.prioirty += 1;
            }

            instances.Sort((x, y) => (y.prioirty.CompareTo(x.prioirty)));
            int curRank = 1;
            for (int i = 0; i < instances.Count; i++)
            {
                if (i != 0 && instances[i - 1].prioirty != instances[i].prioirty)
                    curRank += 1;
                instances[i].rank = curRank;
            }

            UIManager.Instance.RenewDiscoverdItemList(discoveredItems);
        }

        private void Awake()
        {
            if (PhotonNetwork.connected)
                transform.parent = MultiplayRoomManager.Instance.SceneObjParent;
            instances.Add(this);

            orgSprite = GetComponent<SpriteRenderer>().sprite;
            Debug.Log(GetComponent<SpriteRenderer>().color);
        }

        private void Start()
        {
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
            touchArea = new Rect(transform.position.x - touchAreaSize.x / 2, transform.position.y - touchAreaSize.y / 2,
                                                    touchAreaSize.x, touchAreaSize.y);
        }

        [PunRPC]
        public void Init(int itemGenPointIdx)
        {
            genPoint = MapDataManager.Instance.ItemGenPoints[itemGenPointIdx];
            MapDataManager.Instance.ItemGenPoints[itemGenPointIdx].SetItem(this);
        }

        /// <summary>
        /// Change this item's state to stolen.
        /// </summary>
        public void Stolen()
        {
            IsStolen = true;

            // Change sprite to the transparent.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = stolenSprite;
            if (spriteRenderer.isVisible)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;
            }

            isTrapped = false;
            targetSign.SetActive(false);
        }

        /// <summary>
        /// Restore this item's state from stolen to normal.
        /// </summary>
        public void Restored()
        {
            IsStolen = false;

            // Change sprite to the item's original one.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = orgSprite;

            if (spriteRenderer.isVisible)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;
            }
        }

        private void OnBecameVisible()
        {
            if (!discoveredItems.Contains(this))
            {
                if (!IsStolen)
                    discoveredItems.Add(this);
            }
            else
            {
                if (IsStolen)
                    IsStolenChecked = true;
                else
                    IsStolenChecked = false;
            }

            UIManager.Instance.RenewDiscoverdItemList(discoveredItems);
        }

        private void Update()
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch curTouch = Input.GetTouch(i);
                        Vector2 touchedWorldPoint = Camera.main.ScreenToWorldPoint(curTouch.position);
                        if (curTouch.phase == TouchPhase.Ended && touchArea.Contains(touchedWorldPoint))
                        {
                            if (!pickMode)
                                UIManager.Instance.SetItemPopUp(this, Input.GetTouch(i).position);
                            else
                            {
                                SetPickMode(false);
                                pickModeMethod(this);
                            }
                        }
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {

                    Vector2 clickedWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    if (touchArea.Contains(clickedWorldPoint))
                    {
                        if (!pickMode)
                            UIManager.Instance.SetItemPopUp(this, Input.mousePosition);
                        else
                        {
                            SetPickMode(false);
                            pickModeMethod(this);
                        }
                    }
                }
            }
        }

        /*
        public ItemPropStrings GetPropStrings(bool shortMode)
        {
            ItemPropStrings itemPropStrings;

            itemPropStrings.ColorString = "";
            itemPropStrings.AgeString = "";
            itemPropStrings.UsageString = "";

            switch (myColor)
            {
                case EItemColor.Red:
                    itemPropStrings.ColorString = "빨강";
                    break;
                case EItemColor.Blue:
                    itemPropStrings.ColorString = "파랑";
                    break;
                case EItemColor.Yellow:
                    itemPropStrings.ColorString = "노랑";
                    break;
            }

            //Modify age text in pop-up.
            switch (myAge)
            {
                case EItemAge.Ancient:
                    itemPropStrings.AgeString = "고대";
                    break;
                case EItemAge.Middle:
                    itemPropStrings.AgeString = "중근세";
                    break;
                case EItemAge.Modern:
                    itemPropStrings.AgeString = "현대";
                    break;
            }

            //Modify age text in pop-up.
            switch (myUsage)
            {
                case EItemUsage.Art:
                    itemPropStrings.UsageString = "예술";
                    break;
                case EItemUsage.Daily:
                    itemPropStrings.UsageString = "생활";
                    break;
                case EItemUsage.War:
                    itemPropStrings.UsageString = "전쟁";
                    break;
            }

            if (shortMode)
            {
                itemPropStrings.ColorString = itemPropStrings.ColorString.Substring(0, 1);
                itemPropStrings.AgeString = itemPropStrings.AgeString.Substring(0, 1);
                itemPropStrings.UsageString = itemPropStrings.UsageString.Substring(0, 1);
            }

            return itemPropStrings;
        }
        */

        private bool isTrapped = false;
        public bool IsTrapped
        {
            get
            {
                return isTrapped;
            }
        }
        public GameObject targetSign;

        public void SetTrap()
        {
            isTrapped = true;
            targetSign.SetActive(true);
        }

        #region Item Pick Mode For Skills

        public delegate void PickItemMethod(ItemController item);

        static PickItemMethod pickModeMethod = null;
        bool pickMode = false;
        public GameObject pickModeIndicator;

        /// <summary>
        /// Set item pick mode used for skills. Method is called when an item is selected.
        /// </summary>
        /// <param name="method">Called when item is selected while item pick mode is on. 
        ///                     Selected ItemController should be passed as the parameter of this method. </param>
        public static void ActivatePickModeForAllItems(PickItemMethod method)
        {
            pickModeMethod = method;
            foreach (ItemController item in instances)
            {
                if (item.IsStolen) continue;

                item.SetPickMode(true);
            }
        }

        /// <summary>
        /// Deactivate pick item mode.
        /// </summary>
        public static void DeactivatePickModeForAllItems()
        {
            foreach (ItemController item in instances)
                item.SetPickMode(false);
        }

        /// <summary>
        /// Activate/Deactivate item pick mode/indicator for an item.
        /// </summary>
        /// <param name="state">Pick mode activated when true, deactivated when false.</param>
        private void SetPickMode(bool state)
        {
            pickMode = state;
            pickModeIndicator.SetActive(state);
        }

        #endregion
    }
}

