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

        private Transform cameraTransform;

        private void Awake()
        {
            if (instance == null)
                instance = this;

            int curWidth = Screen.width;
            uiScale = (float)curWidth / refWidth;

            moveButtonPanelRect = ConvertToScreenRect(moveButtonPanel);

            arrestPopUp.gameObject.SetActive(false);
            arrestPopUpPanel = arrestPopUp.GetComponent<RectTransform>();

            itemInfoPopUp.gameObject.SetActive(false);
            itemInfoPopUpPanel = itemInfoPopUp.GetComponent<RectTransform>();

            stealPopUp.gameObject.SetActive(false);
            putItemPopUp.gameObject.SetActive(false);

            targetItemList.gameObject.SetActive(false);

            blackPanel.SetActive(false);
            winPanel.SetActive(false);
            losePanel.SetActive(false);

            //Debug.Log(moveButtonPanelRect);
            //Debug.Log(ConvertToScreenRect(arrestPopUpPanel));
        }

        private bool observerMode = false;
        private bool touching = false;
        private Vector3 prevTouchPoint;
        private void Update()
        {
            if (!observerMode)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (Input.touchCount > 0)
                    {
                        for (int i = 0; i < Input.touchCount; i++)
                        {
                            Touch tempTouchs = Input.GetTouch(i);
                            if (tempTouchs.phase == TouchPhase.Began)
                            {
                                if (arrestPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, ConvertToScreenRect(arrestPopUpPanel)))
                                {
                                    arrestPopUp.gameObject.SetActive(false);
                                    arrestPopUpPanel.anchoredPosition = arrestPopUp.OrgAnchorPos;
                                }

                                if (itemInfoPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, Rect.zero))
                                {
                                    itemInfoPopUp.gameObject.SetActive(false);
                                    itemInfoPopUpPanel.anchoredPosition = itemInfoPopUp.OrgAnchorPos;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        //Debug.Log(Input.mousePosition);
                        //if (moveButtonPanelRect.Contains(Input.mousePosition))
                        //Debug.Log("Contained in the mouse panel.");

                        if (arrestPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, ConvertToScreenRect(arrestPopUpPanel)))
                        {
                            arrestPopUp.gameObject.SetActive(false);
                            arrestPopUpPanel.anchoredPosition = arrestPopUp.OrgAnchorPos;
                        }

                        if (itemInfoPopUp.gameObject.GetActive() && IsTouchPointValid(Input.mousePosition, Rect.zero))
                        {
                            itemInfoPopUp.gameObject.SetActive(false);
                            itemInfoPopUpPanel.anchoredPosition = itemInfoPopUp.OrgAnchorPos;
                        }
                    }
                }
            }
            else        // For observer mode(not used now)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Debug.Log("Touch Start");
                    touching = true;
                    prevTouchPoint = Input.mousePosition;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    Debug.Log("Touch End");
                    touching = false;
                }

                if (touching == true)
                {
                    cameraTransform.position += 0.1f * (prevTouchPoint - Input.mousePosition);
                    prevTouchPoint = Input.mousePosition;
                }
            }
        }

        #region Pop-Ups

        public ArrestPopUp arrestPopUp;
        private RectTransform arrestPopUpPanel;
        public void SetArrestPopUp(GameObject selectedChar, Vector3 point)
        {
            if (gameObject == null || !IsTouchPointValid(point, ConvertToScreenRect(arrestPopUpPanel)))
                return;

            ThiefController maybeThief = selectedChar.GetComponent<ThiefController>();
            arrestPopUp.Set(maybeThief);

            arrestPopUp.transform.position = point;
            arrestPopUp.gameObject.SetActive(true);

            //Debug.Log("Arrest Pop-up Rect: " + ConvertToScreenRect(arrestPopUpPanel));
        }

        public ItemInfoPopUp itemInfoPopUp;
        private RectTransform itemInfoPopUpPanel;
        public void SetItemPopUp(ItemController item, Vector3 point)
        {
            if (!IsTouchPointValid(point, Rect.zero))
                return;

            itemInfoPopUp.SetAttributes(item);

            itemInfoPopUp.transform.position = point;
            itemInfoPopUp.gameObject.SetActive(true);
        }

        public StealPopUp stealPopUp;
        public void SetStealPopUp(ItemGenPoint curGenPoint)
        {
            stealPopUp.CurGenPoint = curGenPoint;

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(curGenPoint.transform.position + new Vector3(0, 3, 0));
            stealPopUp.transform.position = screenPoint;
            stealPopUp.gameObject.SetActive(true);
        }

        public void RemoveStealPopUp()
        {
            stealPopUp.CurGenPoint = null;
            stealPopUp.gameObject.SetActive(false);
        }

        public PutItemPopUp putItemPopUp;
        public void SetPutItemPopUp(PutItemPoint curPutPoint)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(curPutPoint.transform.position);
            putItemPopUp.transform.position = screenPoint;
            putItemPopUp.gameObject.SetActive(true);
        }

        public void RemovePutItemPopUp()
        {
            putItemPopUp.gameObject.SetActive(false);
        }

         

        Rect screentRect = new Rect(0, 0, Screen.width, Screen.height);
        /// <summary>
        /// Move pop-up screens following the character's move.
        /// </summary>
        /// <param name="move"></param>
        public void MovePopUpsOnCameraMoving(Vector3 move)
        {
            Vector3 oldWorldPoint;

            // Move character pop-up.
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

            // Move item information pop-up.
            if (itemInfoPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(itemInfoPopUp.transform.position);
                oldWorldPoint -= move;
                itemInfoPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }

            // Move steal pop-up.
            if (stealPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(stealPopUp.transform.position);
                oldWorldPoint -= move;
                stealPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }

            // Move put item pop-up.
            if (putItemPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(putItemPopUp.transform.position);
                oldWorldPoint -= move;
                putItemPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
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
            // Check the touched point is in the area of the pop-up itself.
            if (popUpRect.Contains(touchPoint))
            {
                return false;
            }

            // Check the touched point is in the area of the other UI windows.
            if (moveButtonPanelRect.Contains(touchPoint))
            {
                return false;
            }

            return true;
        }

        #endregion

        #region Item Information Panel

        public Text discoverdItemList;
        public void RenewDiscoverdItemList(List<ItemController> discoveredItems)
        {
            discoverdItemList.text =  "확인한 아이템: ";
            foreach (ItemController item in discoveredItems)
            {
                discoverdItemList.text += "\n" + ItemInfoToString(item);
                if (item.IsStolenChecked)
                {
                    discoverdItemList.text += " 훔";
                }
            }
        }

        /// <summary>
        /// Information panel for items to steal(visible by thieves only)
        /// </summary>
        public Text targetItemList;
        public void RenewTargetItemList(List<ItemController> targetItems)
        {
            targetItemList.gameObject.SetActive(true);

            targetItemList.text = "훔칠 아이템: ";
            foreach (ItemController item in targetItems)
            {
                this.targetItemList.text += "\n" + ItemInfoToString(item);
            }
        }

        /// <summary>
        /// Information panel for items stolen in this game.
        /// </summary>
        public Text stolenItemList;
        public void RenewStolenItemList(List<ItemController> stolenItems)
        {
            stolenItemList.text = "훔쳐진 아이템: ";
            foreach (ItemController item in stolenItems)
            {
                stolenItemList.text += "\n" + ItemInfoToString(item);
            }
        }

        #endregion

        #region Win/Lose Panel

        public GameObject winPanel;
        public GameObject losePanel;

        public void SetWinPanel()
        {
            winPanel.SetActive(true);
            InteractableUIGroup.interactable = false;
        }

        public void SetLosePanel()
        {
            losePanel.SetActive(true);
            InteractableUIGroup.interactable = false;
        }

        #endregion

        public GameObject blackPanel;
        public CanvasGroup InteractableUIGroup;
        public void DeactivateUIGroup()
        {
            InteractableUIGroup.interactable = false;
            blackPanel.SetActive(true);
        }
        
        public void ActivateUIGroup()
        {
            InteractableUIGroup.interactable = true;
            blackPanel.SetActive(false);
        }

        /// <summary>
        /// Label to show the team of the local player.
        /// </summary>
        public Text TeamLabel;
        public void SetTeamLabel(ETeam myTeam)
        {
            switch (myTeam)
            {
                case ETeam.Detective:
                    TeamLabel.text = "탐정";
                    break;
                case ETeam.Thief:
                    TeamLabel.text = "도둑";
                    break;
                default:
                    TeamLabel.text = "오류";
                    break;
            }
        }

        /// <summary>
        /// Label to show remaining thief number
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
        /// Set UI for the arrested thief player.
        /// </summary>
        public void SetObserverModeUI()
        {
            Debug.Log("Start Observer Mode.");

            targetItemList.gameObject.SetActive(false);
            moveButtonPanel.gameObject.SetActive(false);

            observerMode = true;
            cameraTransform = Camera.main.transform;
        }

        /// <summary>
        /// Label to use check error in device environmnet(Should not work in release version)
        /// </summary>
        public Text errorLabel;
        public void SetErrorMsg(string errorMsg)
        {
            errorLabel.text = errorMsg;
        }
    }
}