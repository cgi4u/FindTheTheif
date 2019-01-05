using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class RoutingManager : MonoBehaviour
    {
        //나중에 const변경 필요
        public int maxRoomNum; 
        public int maxFloorNum;

        //Singleton Instance
        private static RoutingManager instance;
        public static RoutingManager Instance
        {
            get
            {
                return instance;
            }
        }

        //각 전시실이 위치하는 층을 저장
        private List<int> roomFloor;
        public List<int> RoomFloor
        {
            get
            {
                return roomFloor;
            }
        }

        //전시실 내부 순환경로를 저장
        private List<Route> inRoomRouteSet;
        public List<Route> InRoomRouteSet
        {
            get
            {
                return inRoomRouteSet;
            }
        }

        //방과 방 사이 경로를 저장
        private List<List<Route>> roomToRoomRouteSet;
        public List<List<Route>> RoomToRoomRouteSet
        {
            get
            {
                return roomToRoomRouteSet;
            }
        }

        //계단->방 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        private List<List<Route>> stairToRoomRouteSet;
        public List<List<Route>> StairToRoomRouteSet
        {
            get
            {
                return stairToRoomRouteSet;
            }
        }

        //방->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        private List<List<Route>> roomToStairRouteSet;
        public List<List<Route>> RoomToStairRouteSet
        {
            get
            {
                return roomToStairRouteSet;
            }
        }

        //계단->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1 
        private List<List<Route>> stairToStairRouteSet;
        public List<List<Route>> StairToStairRouteSet
        {
            get
            {
                return stairToStairRouteSet;
            }
        }

        //현존하는 모든 경로 저장(NPC의 초기위치 설정을 위해)
        private List<Route> allRouteSet;

        private void Awake()
        {
            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");


            //In-Room 루트와 Room-to-Room 루트를 각각 리스트화한다.
            roomFloor = new List<int>();
            inRoomRouteSet = new List<Route>();
            roomToRoomRouteSet = new List<List<Route>>();
            stairToRoomRouteSet = new List<List<Route>>();
            roomToStairRouteSet = new List<List<Route>>();
            stairToStairRouteSet = new List<List<Route>>();
            allRouteSet = new List<Route>();

            for (int i = 0; i < maxRoomNum; i++)
            {
                roomFloor.Add(-1);
                inRoomRouteSet.Add(null);

                List<Route> tempRouteSet = new List<Route>();
                for (int j = 0; j < maxRoomNum; j++)
                    tempRouteSet.Add(null);
                roomToRoomRouteSet.Add(tempRouteSet);

                tempRouteSet = new List<Route>();
                List<Route> tempRouteSet2 = new List<Route>();

                //방->계단과 계단->방 루트 2가지에 각각 상층, 하층 루트 2가지씩 배정
                for (int j = 0; j < 2; j++)
                {
                    tempRouteSet.Add(null);
                    tempRouteSet2.Add(null);
                }
                stairToRoomRouteSet.Add(tempRouteSet);
                roomToStairRouteSet.Add(tempRouteSet2);
            }

            for (int i = 0; i < maxFloorNum; i++)
            {
                List<Route> tempRouteSet = new List<Route>();
                for (int j = 0; j < 4; j++)
                {
                    tempRouteSet.Add(null);
                }
                stairToStairRouteSet.Add(tempRouteSet);
            }

            GameObject roomsRoot
                = transform.Find("Exhibit Rooms").gameObject;
            ExhibitRoom[] roomsArray = roomsRoot.GetComponentsInChildren<ExhibitRoom>();
            foreach (ExhibitRoom room in roomsArray)
            {
                roomFloor[room.num] = room.floor;
            }

            GameObject inRoomRoutesRoot
                = transform.Find("In-Room Routes").gameObject;
            Route[] inRoomRoutesArray = inRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in inRoomRoutesArray)
            {
                inRoomRouteSet[route.curRoom] = route;
            }
           

            GameObject roomToRoomRoutesRoot
                = transform.Find("Room-to-Room Routes").gameObject;
            Route[] roomToRoomRoutesArray = roomToRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in roomToRoomRoutesArray)
            {
                roomToRoomRouteSet[route.startRoom][route.endRoom] = route;
            }

            GameObject stairToRoomRoutesRoot
                = transform.Find("Stair-to-Room Routes").gameObject;
            Route[] stairToRoomRoutesArray = stairToRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in stairToRoomRoutesArray)
            {
                if (route.stairType == Route.StairType.down)
                    stairToRoomRouteSet[route.endRoom][0] = route;
                else
                    stairToRoomRouteSet[route.endRoom][1] = route;
            }

            GameObject roomToStairRoutesRoot
                = transform.Find("Room-to-Stair Routes").gameObject;
            Route[] roomToStairRoutesArray = roomToStairRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in roomToStairRoutesArray)
            {
                if (route.stairType == Route.StairType.down)
                    roomToStairRouteSet[route.startRoom][0] = route;
                else
                    roomToStairRouteSet[route.startRoom][1] = route;
            }

            GameObject stairToStairRoutesRoot
                = transform.Find("Stair-to-Stair Routes").gameObject;
            Route[] stairToStairRouteArray = stairToStairRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in stairToStairRouteArray)
            {
                if (route.stairSide == Route.StairSide.left && route.stairType == Route.StairType.down)
                    stairToStairRouteSet[route.floor][0] = route;
                else if (route.stairSide == Route.StairSide.left && route.stairType == Route.StairType.up)
                    stairToStairRouteSet[route.floor][1] = route;
                else if (route.stairSide == Route.StairSide.right && route.stairType == Route.StairType.down)
                    stairToStairRouteSet[route.floor][2] = route;
                else if (route.stairSide == Route.StairSide.right && route.stairType == Route.StairType.up)
                    stairToStairRouteSet[route.floor][3] = route;
            }
            
            Route[] allRouteArray = GetComponentsInChildren<Route>();
            for (int i = 0; i < allRouteArray.Length; i++)
            {
                if (allRouteArray[i].routeType != Route.RouteType.Stair_to_Stair
                    && allRouteArray[i].routeType != Route.RouteType.Room_to_Stair)
                {
                    allRouteSet.Add(allRouteArray[i]);
                }
            }
            ifRouteAssigned = new bool[allRouteSet.Count];

            Debug.Log("All selectable routes: " + allRouteSet.Count);
        }

        #region Public Methods

        private bool[] ifRouteAssigned;
        public Route getRandomRoute()
        {
            int randIdx;
            do
            {
                randIdx = Random.Range(0, allRouteSet.Count);
            } while (ifRouteAssigned[randIdx]);

            return allRouteSet[randIdx];
        }

        #endregion
    }
}
