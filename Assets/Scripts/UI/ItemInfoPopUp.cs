using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    using static ItemProperties;

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
            colorText.text = UITextForProp(item.Color, false);
            ageText.text = UITextForProp(item.Age, false);
            usageText.text = UITextForProp(item.Usage, false);
        }
    }
}
