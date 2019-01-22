using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // 각 경로의 타입과 노드 집합을 저장하는 객체
    public class Route : MonoBehaviour
    {
        // 경로 타입
        // 방 내 순환경로, 같은 층에서의 방과 방 사이 이동경로, 방에서 계단으로의 경로, 계단에서 방으로의 경로
        public enum RouteType { In_Room, Room_to_Room, Room_to_Stair, Stair_to_Room, Stair_to_Stair }  
        public RouteType routeType;

        // 경로에 포함되어 있는 노드 집합
        // NPC가 참조해서 경로를 따라가는 데 사용
        [SerializeField]
        private RouteNode[] nodeSet;
        public RouteNode[] NodeSet
        {
            get
            {
                return nodeSet;
            }
        }

        //For room-to-room
        public int startRoom;
        public int endRoom;

        //For in-room
        public int curRoom;

        //For stair-to-room or room-to-stair
        public enum StairType { up, down, none }
        public enum StairSide { left, right }

        public StairType stairType;
        public StairSide stairSide;

        //For stair-to-stair
        public int floor;

        private void Awake()
        {
            // 노드 집합 초기화
            //nodeSet = GetComponentsInChildren<RouteNode>();
        }
    }
}