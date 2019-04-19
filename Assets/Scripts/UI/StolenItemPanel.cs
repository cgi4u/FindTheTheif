using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    using static ItemProperties;

    public class StolenItemPanel : MonoBehaviour
    {
        public StolenItemUnitPanel[] unitPanels;

        class PropertyCount
        {
            public string Name;
            public Color Color;
            public int Count;
        }

        List<PropertyCount> counts = new List<PropertyCount>();

        public void Renew(List<ItemController> stolenItems)
        {
            counts.Clear();
            for (int i = 0; i < ItemPropTypeNum * ItemPropNumPerType; i++)
                counts.Add(new PropertyCount());

            foreach (ItemController item in stolenItems)
            {
                counts[CodeForProp(item.Color)].Name = UITextForProp(item.Color, true);
                counts[CodeForProp(item.Color)].Color = UIColorForProp(item.Color);
                counts[CodeForProp(item.Color)].Count += 1;

                counts[CodeForProp(item.Age)].Name = UITextForProp(item.Age, true);
                counts[CodeForProp(item.Age)].Color = UIColorForProp(item.Age);
                counts[CodeForProp(item.Age)].Count += 1;

                counts[CodeForProp(item.Usage)].Name = UITextForProp(item.Usage, true);
                counts[CodeForProp(item.Usage)].Color = UIColorForProp(item.Usage);
                counts[CodeForProp(item.Usage)].Count += 1;
            }

            counts.Sort((x, y) => y.Count.CompareTo(x.Count));
            for (int i = 0; i < counts.Count; i++)
            {
                if (counts[i].Count == 0)
                    break;
                unitPanels[i].Set(counts[i].Color, counts[i].Name, counts[i].Count);
            }
        }
    }
}
