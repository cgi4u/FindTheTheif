using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    /// <summary>
    /// Container for routes realted to stair(to/from Stair, Stair to Stair)
    /// </summary>
    [System.Serializable]
    public class StairRouteContainer
    {
        [SerializeField]
        private Route leftDownRoute;
        public Route LeftDownRoute
        {
            get
            {
                return leftDownRoute;
            }
        }
        [SerializeField]
        private Route leftUpRoute;
        public Route LeftUpRoute
        {
            get
            {
                return leftUpRoute;
            }
        }
        [SerializeField]
        private Route rightDownRoute;
        public Route RightDownRoute
        {
            get
            {
                return rightDownRoute;
            }
        }
        [SerializeField]
        private Route rightUpRoute;
        public Route RightUpRoute
        {
            get
            {
                return rightUpRoute;
            }
        }
    }
}
