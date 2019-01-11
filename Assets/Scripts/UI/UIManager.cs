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

        void Start()
        {
            if (instance == null)
                instance = this;

            charPopUp.SetActive(false);
            itemPopUp.gameObject.SetActive(false);
        }

        void Update()
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
                remainingTheifLabel.text = "남은 도둑: " + MultiplayRoomManager.Instance.RemainingTheif;
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

        public void MovePopUpsOnCameraMoving(Vector3 move)
        {
            Vector3 oldWorldPoint;
            if (charPopUp.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(charPopUp.transform.position);
                oldWorldPoint -= move;
                charPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }
            if (itemPopUp.gameObject.GetActive())
            {
                oldWorldPoint = Camera.main.ScreenToWorldPoint(itemPopUp.transform.position);
                oldWorldPoint -= move;
                itemPopUp.transform.position = Camera.main.WorldToScreenPoint(oldWorldPoint);
            }
        }

        #endregion
    }
}