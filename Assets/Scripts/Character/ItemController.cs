using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class ItemController : Photon.PunBehaviour
    {
        #region Item Properties

        public static readonly int ItemPropTypeNum = 3;
        public static readonly int ItemPropNumPerType = 3;

        /*
        public static string GetItemPropName(int type, int num, bool shortMode)
        {
            switch (type)
            {
                case 0:
                    switch (num)
                    {
                        case 0:
                            if (shortMode)
                                return "빨";
                            else
                                return "빨강";
                        case 1:
                            if (shortMode)
                                return "파";
                            else
                                return "파랑";
                        case 2:
                            if (shortMode)
                                return "노";
                            else
                                return "노랑";
                        default:
                            Debug.LogError("Invalid item property value.");
                            return null;
                    }
                case 1:
                    switch (num)
                    {
                        case 0:
                            if (shortMode)
                                return "고";
                            else
                                return "고대";
                        case 1:
                            if (shortMode)
                                return "";
                            else
                                return "파랑";
                        case 2:
                            if (shortMode)
                                return "노";
                            else
                                return "노랑";
                        default:
                            Debug.LogError("Invalid item property value.");
                            return null;
                    }
                    break;
                case 2:
                    break;
            }
        }
        */

        #endregion


        /// <summary>
        /// The list  of items that the local player discovered in game.
        /// </summary>
        static List<ItemController> discoveredItems = new List<ItemController>();

        [SerializeField]
        private Sprite orgSprite;
        [SerializeField]
        protected Sprite transparentSprite;

        [SerializeField]
        private Vector2 touchAreaSize;
        private Rect touchArea;

        public bool IsStolen { get; set; } = false;
        public bool IsStolenChecked { get; set; } = false;

        #region Intrinsic Item Properties

        [SerializeField]
        private ItemColor myColor;
        public ItemColor Color
        {
            get
            {
                return myColor;
            }
        }

        [SerializeField]
        private ItemAge myAge;
        public ItemAge Age
        {
            get
            {
                return myAge;
            }
        }

        [SerializeField]
        private ItemUsage myUsage;
        public ItemUsage Usage
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
        public int Prioirty = 0;

        #endregion

        /// <summary>
        /// Reset discovered / stolen item list for each game.
        /// </summary>
        public static void ResetDescoverdItems()
        {
            discoveredItems.Clear();
        }

        /// <summary>
        /// Renew the print prioirty of the discoverd item list(when an item is stolen)
        /// </summary>
        /// <param name="stolen"></param>
        public static void RenewDiscoveredItemPrioirty(ItemController stolen)
        {
            if (discoveredItems.Contains(stolen))
                discoveredItems.Remove(stolen);

            foreach (ItemController discovered in discoveredItems)
            {
                if (discovered.Color == stolen.Color)
                    discovered.Prioirty += 1;
                if (discovered.Age == stolen.Age)
                    discovered.Prioirty += 1;
                if (discovered.Usage == stolen.Usage)
                    discovered.Prioirty += 1;
            }

            discoveredItems.Sort((x, y) => (y.Prioirty.CompareTo(x.Prioirty)));
            UIManager.Instance.RenewDiscoverdItemList(discoveredItems);
        }

        private void Awake()
        {
            if (PhotonNetwork.connected)
                transform.parent = MultiplayRoomManager.Instance.SceneObjParent;

            orgSprite = GetComponent<SpriteRenderer>().sprite;
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

        [PunRPC]
        public void Stolen()
        {
            IsStolen = true;

            // Change sprite to the transparent.
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = transparentSprite;
            if (spriteRenderer.isVisible)
            {
                spriteRenderer.enabled = false;
                spriteRenderer.enabled = true;
            }
        }

        [PunRPC]
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
                {
                    Debug.Log(gameObject.name + "is visible.");
                    discoveredItems.Add(this);
                }
            }
            else
            {
                if (IsStolen)
                {
                    IsStolenChecked = true;
                }
                else
                {
                    IsStolenChecked = false;
                }
            }

            UIManager.Instance.RenewDiscoverdItemList(discoveredItems);
        }

        /*
        private void OnMouseUp()
        {
            UIManager.Instance.SetItemPopUp(this, Input.mousePosition);
        }
        */

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
                            UIManager.Instance.SetItemPopUp(this, Input.GetTouch(i).position);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 clickedWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Debug.Log(clickedWorldPoint);
                    if (touchArea.Contains(clickedWorldPoint))
                        UIManager.Instance.SetItemPopUp(this, Input.mousePosition);
                }
            }
        }
    }
}

