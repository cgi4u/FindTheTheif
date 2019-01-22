﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.MJT.FindTheTheif
{ 
    public class ItemPopUp : MonoBehaviour
    {
        public Text colorText;
        public Text ageText;
        public Text usageText;

        public void SetAttributes(ItemColor color, ItemAge age, ItemUsage usage)
        {
            //Modify color text in pop-up.
            switch (color)
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
            switch (age)
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
            switch (usage)
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