using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// Container for routes realted to stair(to/from Stair, Stair to Stair)
    /// </summary>
    [System.Serializable]
    public class StairRouteContainer
    {
        [SerializeField]
        private Route[] leftDownRoutes;
        public Route[] LeftDownRoutes
        {
            get
            {
                return leftDownRoutes;
            }
        }

        [SerializeField]
        private Route[] leftUpRoutes;
        public Route[] LeftUpRoutes
        {
            get
            {
                return leftUpRoutes;
            }
        }

        [SerializeField]
        private Route[] rightDownRoutes;
        public Route[] RightDownRoutes
        {
            get
            {
                return rightDownRoutes;
            }
        }

        [SerializeField]
        private Route[] rightUpRoutes;
        public Route[] RightUpRoutes
        {
            get
            {
                return rightUpRoutes;
            }
        }

        public Route[] RoutesWithSideAndType(EStairSide stairSide, EStairType stairType)
        {
            if (stairSide  == EStairSide.Left && stairType == EStairType.Down)
            {
                return leftDownRoutes;
            }
            else if (stairSide == EStairSide.Left && stairType == EStairType.Up)
            {
                return leftUpRoutes;
            }
            else if (stairSide == EStairSide.Right && stairType == EStairType.Down)
            {
                return rightDownRoutes;
            }
            else
            {
                return rightUpRoutes;
            }
        }
    }
}
