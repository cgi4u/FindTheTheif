using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    /// <summary>
    /// Store datas for item(If it is an item watching point or not, item direction)
    /// </summary>
    public class RouteNode : MonoBehaviour
    {
        [SerializeField]
        private bool ifItemPoint;
        public bool IfItemPoint
        {
            get
            {
                return ifItemPoint;
            }
        }
        [SerializeField]
        private Direction itemDir;
        public Direction ItemDir
        {
            get
            {
                return itemDir;
            }
        }
    }
}
