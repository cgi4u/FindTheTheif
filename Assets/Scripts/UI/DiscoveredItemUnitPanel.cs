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
        public GameObject undiscoverdMark;

        public Image spriteImage;
        public Text floorAndRoom;
        public Text rank;

        public Image colorPropPanel;
        public Text colorPropText;

        public Image agePropPanel;
        public Text agePropText;

        public Image usagePropPanel;
        public Text usagePropText;

        public void SetItemInfo(ItemController item)
        {
            undiscoverdMark.SetActive(false);

            spriteImage.gameObject.SetActive(true);
            spriteImage.sprite = item.GetComponent<SpriteRenderer>().sprite;
            floorAndRoom.gameObject.SetActive(true);
            floorAndRoom.text = item.GenPoint.Room.Floor + " - " + item.GenPoint.Room.Num;
            rank.gameObject.SetActive(true);
            rank.text = item.Rank.ToString();

            ItemPropStrings itemPropStrings = item.GetPropStrings(true);

            colorPropPanel.gameObject.SetActive(true);
            colorPropPanel.color = UIColorForProp(item.Color);
            colorPropText.gameObject.SetActive(true);
            colorPropText.text = itemPropStrings.ColorString;

            agePropPanel.gameObject.SetActive(true);
            agePropPanel.color = UIColorForProp(item.Age);
            agePropText.gameObject.SetActive(true);
            agePropText.text = itemPropStrings.AgeString;

            usagePropPanel.gameObject.SetActive(true);
            usagePropPanel.color = UIColorForProp(item.Usage);
            usagePropText.gameObject.SetActive(true);
            usagePropText.text = itemPropStrings.UsageString;
        }
    }
}