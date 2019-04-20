using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    using static ItemProperties;

    [RequireComponent(typeof(RectTransform))]
    public class DiscoveredItemUnitPanel : MonoBehaviour
    {
        bool discovered = false;
        public GameObject undiscoverdMark;

        public Image spriteImage;

        public GameObject floorAndRoomPanel;
        public Text floorAndRoomText;

        public GameObject rankPanel;
        public Text rankText;

        public Image colorPropPanel;
        public Text colorPropText;

        public Image agePropPanel;
        public Text agePropText;

        public Image usagePropPanel;
        public Text usagePropText;

        public void SetItemInfo(ItemController item)
        {
            rankPanel.SetActive(true);
            string ordinalRank;
            switch (item.Rank % 4)
            {
                case 1:
                    ordinalRank = item.Rank + "st";
                    break;
                case 2:
                    ordinalRank = item.Rank + "nd";
                    break;
                case 3:
                    ordinalRank = item.Rank + "rd";
                    break;
                default:
                    ordinalRank = item.Rank + "th";
                    break;
            }
            rankText.text = ordinalRank;

            if (discovered) return;

            discovered = true;
            undiscoverdMark.SetActive(false);

            spriteImage.gameObject.SetActive(true);
            spriteImage.sprite = item.GetComponent<SpriteRenderer>().sprite;

            floorAndRoomPanel.SetActive(true);
            floorAndRoomText.text = item.GenPoint.Room.Floor + " - " + item.GenPoint.Room.Num;

            colorPropPanel.gameObject.SetActive(true);
            colorPropPanel.color = UIColorForProp(item.Color);
            colorPropText.gameObject.SetActive(true);
            colorPropText.text = UITextForProp(item.Color, true);

            agePropPanel.gameObject.SetActive(true);
            agePropPanel.color = UIColorForProp(item.Age);
            agePropText.gameObject.SetActive(true);
            agePropText.text = UITextForProp(item.Age, true);

            usagePropPanel.gameObject.SetActive(true);
            usagePropPanel.color = UIColorForProp(item.Usage);
            usagePropText.gameObject.SetActive(true);
            usagePropText.text = UITextForProp(item.Usage, true);
        }

        public GameObject stolenMark;

        public void SetStolenMark()
        {
            stolenMark.SetActive(true);
        }

        public void RemoveStolenMark()
        {
            stolenMark.SetActive(false);
        }

        public GameObject targetMark;

        public void SetAsTarget()
        {
            GetComponent<Image>().color = Color.red;
            targetMark.SetActive(true);
        }
    }
}