using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class Route : MonoBehaviour
    {
        public enum RouteType { In_Room, Room_to_Room, Room_to_Stair, Stair_to_Room }

        public RouteType routeType;

        private RouteNode[] nodeSet;
        public RouteNode[] NodeSet
        {
            get
            {
                return nodeSet;
            }
        }

        //If room-to-room
        public int startRoom;
        public int endRoom;

        //if in-room
        public int curRoom;

        private void Awake()
        {
            nodeSet = GetComponentsInChildren<RouteNode>();
        }

        //if stair-to-room or room-to-stair
        public enum StairType { up, down }

        public StairType stairType;
        public int room;
    }
}