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

        void Start()
        {
            if (instance == null)
                instance = this;

            charPopUp.SetActive(false);
            itemPopUp.gameObject.SetActive(false);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
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
                timeLabel.text = Mathf.Floor(MultiplayRoomManager.Instance.timeLeft).ToString();
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

        public void SetItemPopUp(string[] attributes, Vector3 objPos)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
            //itemPopUp.gameObject.
            itemPopUp.transform.position = screenPoint;
            itemPopUp.SetAttributes(attributes);
            itemPopUp.gameObject.SetActive(true);
        }
        #endregion
    }
}