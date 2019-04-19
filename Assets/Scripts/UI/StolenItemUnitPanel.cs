using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    public class StolenItemUnitPanel : MonoBehaviour
    {
        public Image propertyPanel;
        public Text propertyName;
        public Text propertyCount;

        public void Set(Color color, string name, int count)
        {
            gameObject.SetActive(true);
            propertyPanel.color = color;
            propertyName.text = name;
            propertyCount.text = ":" + count;
        }
    }
}
