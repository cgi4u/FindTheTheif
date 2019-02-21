using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class ItemInfoPopUp : MonoBehaviour
    {
        public Text colorText;
        public Text ageText;
        public Text usageText;

        private Vector3 orgAnchorPos;
        public Vector3 OrgAnchorPos
        {
            get
            {
                return orgAnchorPos;
            }
        }

        private void Awake()
        {
            orgAnchorPos = GetComponent<RectTransform>().anchoredPosition;
        }

        public void SetAttributes(ItemController item)
        {
            //Modify color text in pop-up.
            switch (item.Color)
            {
                case ItemColor.Red:
                    colorText.text = "빨강";
                    break;
                case ItemColor.Blue:
                    colorText.text = "파랑";
                    break;
                case ItemColor.Yellow:
                    colorText.text = "노랑";
                    break;
            }
            
            //Modify age text in pop-up.
            switch (item.Age)
            {
                case ItemAge.Ancient:
                    ageText.text = "고대";
                    break;
                case ItemAge.Middle:
                    ageText.text = "중근세";
                    break;
                case ItemAge.Modern:
                    ageText.text = "현대";
                    break;
            }

            //Modify age text in pop-up.
            switch (item.Usage)
            {
                case ItemUsage.Art:
                    usageText.text = "예술";
                    break;
                case ItemUsage.Daily:
                    usageText.text = "생활";
                    break;
                case ItemUsage.War:
                    usageText.text = "전쟁";
                    break;
            }
        }
    }
}
