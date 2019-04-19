using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class DiscoveredItemPanel : MonoBehaviour
    {
        public DiscoveredItemUnitPanel[] unitPanels;

        public void Renew(List<ItemController> discoveredItems, int floor)
        {
            foreach (ItemController item in discoveredItems)
            {
                if (item.GenPoint.Room.Floor == floor)
                {
                    int idx = (item.GenPoint.Room.Num - 1) * 3 + item.GenPoint.Num - 1;
                    unitPanels[idx].SetItemInfo(item);

                    if (item.IsStolenChecked)
                        unitPanels[idx].SetStolenMark();
                    else
                        unitPanels[idx].RemoveStolenMark();
                }
            }
        }
    }
}
