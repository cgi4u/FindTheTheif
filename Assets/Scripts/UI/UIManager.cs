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

        public int refWidth;
        private float uiScale;

        /// <summary>
        /// That panel that contains move buttons.
        /// </summary>
        public RectTransform moveButtonPanel;
        private Rect moveButtonPanelRect;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            int curWidth = Screen.width;
            uiScale = (float)curWidth / refWidth;

            moveButtonPanelRect = ConvertToScreenRect(moveButtonPanel);

            arrestPopUp.gameObject.SetActive(false);
            arrestPopUpPanel = arrestPopUp.GetComponent<RectTransform>();

            itemPopUp.gameObject.SetActive(false);
            itemPopUpPanel = itemPopUp.GetComponent<RectTransform>();

            stealPopUp.gameObject.SetActive(false);

            targetItemList.gameObject.SetActive(false);

            //Debug.Log(moveButtonPanelRect);
            //Debug.Log(ConvertToScreenRect(arrestPopUpPanel));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log(Input.mousePosition);
                //if (moveButtonPanelRect.Contains(Input.mousePosition))
                    //Debug.Log("Contained in the mouse panel.");

                if (arrestPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, ConvertToScreenRect(arrestPopUpPanel)))
                {
                    arrestPopUp.gameObject.SetActive(false);
                    arrestPopUpPanel.anchoredPosition = arrestPopUp.OrgAnchorPos;
                }

                if (itemPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, Rect.zero))
                {
                    itemPopUp.gameObject.SetActive(false);
                    itemPopUpPanel.anchoredPosition = itemPopUp.OrgAnchorPos;
                }
            }
        }

        #region Pop-up(for NPC/Thieves and for items, Vector3 point)

        public ArrestPopUp arrestPopUp;
        private RectTransform arrestPopUpPanel;
        public void SetArrestPopUp(GameObject unknown, Vector3 point)
        {
            if (gameObject == null || !IsTouchPointValid(point, ConvertToScreenRect(arrestPopUpPanel)))
                return;

            PlayerController maybeTheif = unknown.GetComponent<PlayerController>();

            bool isThief = false;
            int thiefID = -1;
            if (maybeTheif != null)
            {
                isThief = true;
                thiefID = maybeTheif.GetComponent<PhotonView>().ownerId;
            }
            arrestPopUp.Set(isThief, thiefID);

            arrestPopUp.transform.position = point;
            arrestPopUp.gameObject.SetActive(true);

            //Debug.Log("Arrest Pop-up Rect: " + ConvertToScreenRect(arrestPopUpPanel));
        }

        public ItemPopUp itemPopUp;
        private RectTransform itemPopUpPanel;
        public void SetItemPopUp(ItemController item, Vector3 point)
        {
            if (!IsTouchPointValid(point, Rect.zero))
                return;

            itemPopUp.SetAttributes(item);

            itemPopUp.transform.position = point;
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
            if (arrestPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(arrestPopUp.transform.position);
                oldWorldPoint -= move;
                arrestPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);


                if (!screentRect.Contains(arrestPopUp.transform.position))
                {
                    arrestPopUp.gameObject.SetActive(false);
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

        /// <summary>
        /// Used to check the touched point is valid for activation/deactivation of a pop-up.
        /// If the touched point is in the area of other UI windows(ex) Item information) or pop-up itself, returns false(not valid).
        /// </summary>
        /// <param name="touchPoint">The point touched by the user.</param>
        /// <param name="popUpRect">The screen area of the caller pop-up itself.</param>
        /// <returns></returns>
        private bool IsTouchPointValid(Vector3 touchPoint, Rect popUpRect)
        {
            Debug.Log("Touched point: " + touchPoint);

            // Check the touched point is in the area of the pop-up itself.
            Debug.Log("Pop-up Rect: " + popUpRect);
            if (popUpRect.Contains(touchPoint))
            {
                Debug.Log("Touched point is in the pop-up area.");
                return false;
            }

            // Check the touched point is in the area of the other UI windows.
            Debug.Log("Move Button Panel Rect: " + moveButtonPanelRect);
            if (moveButtonPanelRect.Contains(touchPoint))
            {
                Debug.Log("Touched point is in the move button area.");
                return false;
            }

            return true;
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

        //issue 고해상도에 대응하려면 anchor 값들을 사용해서 rectTransform의 실제 position값을 구해줘야함.
        private Rect ConvertToScreenRect(RectTransform rectTransform)
        {
            Vector2 size = rectTransform.rect.size;
            return new Rect((Vector2)rectTransform.position - (size * 0.5f), size);
        }

        /// <summary>
        /// Set UI for the arrested theif user.
        /// </summary>
        public void SetObserverModeUI()
        {
            stealPopUp.gameObject.SetActive(false);
            moveButtonPanel.gameObject.SetActive(false);
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