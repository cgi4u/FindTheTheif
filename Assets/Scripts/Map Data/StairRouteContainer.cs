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
    }
}
