using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.MJT.FindTheThief
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

        /// <summary>
        /// A panel that has movement buttons
        /// </summary>
        public RectTransform moveButtonPanel;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            charPopUp.SetActive(false);
            itemPopUp.gameObject.SetActive(false);
            stealPopUp.gameObject.SetActive(false);
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
        }

        #region Pop-up(for NPC/Thieves and for items)

        public GameObject charPopUp;
        public void SetCharPopUp(int playerID, Vector3 objPos)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
            charPopUp.transform.position = screenPoint;
            charPopUp.SetActive(true);
        }

        public ItemPopUp itemPopUp;
        public void SetItemPopUp(ItemController item)
        {
            itemPopUp.SetAttributes(item);

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(item.transform.position);
            itemPopUp.transform.position = screenPoint;
            
            itemPopUp.gameObject.SetActive(true);
        }

        public StealPopUp stealPopUp;
        public void SetStealPopUp(ItemGenPoint curGenPoint)
        {
            stealPopUp.CurGenPoint = curGenPoint;

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(curGenPoint.transform.position);
            stealPopUp.transform.position = screenPoint;

            stealPopUp.gameObject.SetActive(true);
        }

        public void RemoveStealPopUp()
        {
            stealPopUp.CurGenPoint = null;
            stealPopUp.gameObject.SetActive(false);
        }
         

        Rect screentRect = new Rect(0, 0, Screen.width, Screen.height);
        /// <summary>
        /// Move pop-up screens following the character's move.
        /// </summary>
        /// <param name="move"></param>
        public void MovePopUpsOnCameraMoving(Vector3 move)
        {
            Vector3 oldWorldPoint;

            // Move character Pop-up.
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

            // Move item Pop-up.
            if (itemPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(itemPopUp.transform.position);
                oldWorldPoint -= move;
                itemPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }

            // Move Item Steal Pop-up
            if (stealPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(stealPopUp.transform.position);
                oldWorldPoint -= move;
                stealPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
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

        /// <summary>
        /// Label to show the team of the local player.
        /// </summary>
        public Text TeamLabel;
        public void SetTeamLabel(Team myTeam)
        {
            switch (myTeam)
            {
                case Team.Detective:
                    TeamLabel.text = "탐정";
                    break;
                case Team.Thief:
                    TeamLabel.text = "도둑";
                    break;
                default:
                    TeamLabel.text = "오류";
                    break;
            }
        }

        /// <summary>
        /// Label to show remaining theif number
        /// </summary>
        public Text thievesNumLabel;
        public void RenewThievesNum(int thievesNum)
        {
            thievesNumLabel.text = "남은 도둑: " + thievesNum;
        }

        private string ItemInfoToString(ItemController item)
        {
            string itemInfo = "";

            //Modify color text in pop-up.
            switch (item.Color)
            {
                case ItemColor.Red:
                    itemInfo += "빨 ";
                    break;
                case ItemColor.Blue:
                    itemInfo += "파 ";
                    break;
                case ItemColor.Yellow:
                    itemInfo += "노 ";
                    break;
            }

            //Modify age text in pop-up.
            switch (item.Age)
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
            switch (item.Usage)
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

            itemInfo += item.GenPoint.Room.Floor + "층 " + item.GenPoint.Room.Num + "번 방";

            return itemInfo;
        }

        /// <summary>
        /// Label to show remaining game time
        /// </summary>
        public Text timeLabel;
        public void RenewTimeLabel(int time)
        {
            timeLabel.text = "남은 시간: " + time;
        }

        /// <summary>
        /// Label to use check error in device environmnet(Should not work in release version)
        /// </summary>
        public Text errorLabel;
        public void RenewErrorLabel(string errorMsg)
        {
            errorLabel.text = errorMsg;
        }
    }
}