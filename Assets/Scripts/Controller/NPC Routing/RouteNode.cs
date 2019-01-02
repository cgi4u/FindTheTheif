using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class RouteNode : MonoBehaviour
    {
        public enum Direction { up, down, left, right };

        public bool ifItemPoint;
        public Direction itemDir;
    }
}
