﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// Store datas for NPC moving route.
    /// </summary>
    public class Route : MonoBehaviour
    {
        [SerializeField]
        private ERouteType routeType;
        /// <summary>
        /// Type of this route(In Room, Room to Room, Stair to Room, Room to Stair, Stair to Stair)
        /// </summary>
        public ERouteType RouteType
        {
            get
            {
                return routeType;
            }
        }

        private RouteNode[] nodeSet;
        /// <summary>
        /// The set of nodes which are contained in this route. NPC follows there positions.
        /// </summary>
        public RouteNode[] NodeSet
        {
            get
            {
                return nodeSet;
            }
        }

        #region In Room Route Properties

        [SerializeField]
        private int curRoom;
        /// <summary>
        /// The room contains this route.(used by In Room Routes)
        /// </summary>
        public int CurRoom
        {
            get
            {
                return curRoom;
            }
        }

        #endregion

        #region Room to Room Route Properties

        [SerializeField]
        private int startRoom;
        /// <summary>
        /// The room where this route starts.(used by Room to Room and Room to Stair type Routes)
        /// </summary>
        public int StartRoom
        {
            get
            {
                return startRoom;
            }
        }

        [SerializeField]
        private int endRoom;
        /// <summary>
        /// The room where this route ends.(used by Room to Room and Stair to Room type Routes)
        /// </summary>
        public int EndRoom
        {
            get
            {
                return endRoom;
            }
        }

        #endregion

        #region Stair to Room / Room to Stair Route Properties

        [SerializeField]
        private EStairType stairType;
        /// <summary>
        /// The type of stair(up, down) where this route starts or ends.(used by Stair to Room and Room to Stair Routes)
        /// </summary>
        public EStairType StairType
        {
            get
            {
                return stairType;
            }
        }
        
        [SerializeField]
        private EStairSide stairSide;
        /// <summary>
        /// The side of stair(left, right) where this route starts or ends.(used by Stair to Room and Room to Stair Routes)
        /// </summary>
        public EStairSide StairSide
        {
            get
            {
                return stairSide;
            }
        }

        #endregion

        private void Awake()
        {
            nodeSet = GetComponentsInChildren<RouteNode>();
        }
    }
}