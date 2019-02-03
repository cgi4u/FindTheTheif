using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 1. UI를 띄우고 죽이는 컨트롤러의 경우 가능하면 각 오브젝트가 각자 가지고 알아서 처리하도록 하는게 바람직하다고 생각
// 2. 그러나 UI의 종류와 상관없이 다른 영역을 터치하면 제거해야 하기 때문에 중앙 관리해줄 UI 컨트롤러가 필요해 제작
// 3. 그 때문에 다소 난잡해진 감이 있어 주의해야함.

namespace com.MJT.FindTheTheif
{
    public class UIManager : MonoBehaviour
    {
        static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                return instance;
            }
        }

        //Label to show reamaing game time
        public Text timeLabel;
        //Label to show remaining theif number
        public Text remainingTheifLabel;

        public RectTransform moveButtonPanel;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            charPopUp.SetActive(false);
            itemPopUp.gameObject.SetActive(false);
            targetItemList.gameObject.SetActive(false);
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)
                && (Input.mousePosition.x < moveButtonPanel.rect.xMin || Input.mousePosition.x > moveButtonPanel.rect.xMax)
                && (Input.mousePosition.y < moveButtonPanel.rect.yMin || Input.mousePosition.y > moveButtonPanel.rect.yMax))
            {
                if (charPopUp.GetActive())
                {
                    charPopUp.SetActive(false);
                }
                if (itemPopUp.gameObject.GetActive())
                {
                    itemPopUp.gameObject.SetActive(false);
                }
            }

            if (MultiplayRoomManager.Instance != null)
            {
                timeLabel.text = Mathf.Floor(MultiplayRoomManager.Instance.timeLeft).ToString();
            }
        }

        #region Character(Theif, NPC) Pop-up
        public GameObject charPopUp;

        public void SetCharPopUp(int playerID, Vector3 objPos)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
            charPopUp.transform.position = screenPoint;
            charPopUp.SetActive(true);
        }
        #endregion

        #region Item Pop-up
        public ItemPopUp itemPopUp;

        public void SetItemPopUp(ItemColor itemColor, ItemAge itemAge, ItemUsage itemUsage, Vector3 objPos)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
            //itemPopUp.gameObject.
            itemPopUp.transform.position = screenPoint;
            itemPopUp.SetAttributes(itemColor, itemAge, itemUsage);
            itemPopUp.gameObject.SetActive(true);
        }

        Rect screentRect = new Rect(0, 0, Screen.width, Screen.height);
        public void MovePopUpsOnCameraMoving(Vector3 move)
        {
            Vector3 oldWorldPoint;
            if (charPopUp.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(charPopUp.transform.position);
                oldWorldPoint -= move;
                charPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);

                
                if (!screentRect.Contains(charPopUp.transform.position))
                {
                    charPopUp.SetActive(false);
                }
            }
            if (itemPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(itemPopUp.transform.position);
                oldWorldPoint -= move;
                itemPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }
        }

        #endregion

        #region Item Information Panel

        public Text checkedItemListText;
        List<ItemController> checkedItemList = new List<ItemController>();
        public void AddCheckedItem(ItemController newItem)
        {
            if (!checkedItemList.Contains(newItem))
                checkedItemList.Add(newItem);

            checkedItemListText.text =  "확인한 아이템: ";
            foreach (ItemController item in checkedItemList)
            {
                checkedItemListText.text += "\n" + ItemInfoToString(item);
            }
        }

        //Information to items to steal(visible by theives only)
        public Text targetItemList;
        public void RenewTargetItemList(List<ItemController> targetItems, int targetItemNum, bool[] isItemStolen)
        {
            targetItemList.gameObject.SetActive(true);

            this.targetItemList.text = "훔칠 아이템: ";
            foreach (ItemController item in targetItems)
            {
                this.targetItemList.text += "\n" + ItemInfoToString(item);
            }
        }

        #endregion

        public void RenewThievesNum(int thievesNum)
        {
            remainingTheifLabel.text = "남은 도둑: " + thievesNum;
        }

        private string ItemInfoToString(ItemController item)
        {
            string itemInfo = "";

            //Modify color text in pop-up.
            switch (item.myColor)
            {
                case ItemColor.Red:
                    itemInfo += "빨 ";
                    break;
                case ItemColor.Blue:
                    itemInfo += "파 ";
                    break;
                case ItemColor.Yellow:
                    itemInfo += "노";
                    break;
            }

            //Modify age text in pop-up.
            switch (item.myAge)
            {
                case ItemAge.Ancient:
                    itemInfo += "고 ";
                    break;
                case ItemAge.Middle:
                    itemInfo += "중 ";
                    break;
                case ItemAge.Modern:
                    itemInfo += "현 ";
                    break;
            }

            //Modify age text in pop-up.
            switch (item.myUsage)
            {
                case ItemUsage.Art:
                    itemInfo += "예 ";
                    break;
                case ItemUsage.Daily:
                    itemInfo += "생 ";
                    break;
                case ItemUsage.War:
                    itemInfo += "전 ";
                    break;
            }

            itemInfo += item.FloorNum + "층 " + item.RoomNum + "번 방";

            return itemInfo;
        }

    }
}