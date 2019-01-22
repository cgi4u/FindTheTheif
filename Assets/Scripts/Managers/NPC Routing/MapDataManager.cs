using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class MapDataManager : MonoBehaviour
    {
        //나중에 const변경 필요
        public int maxRoomNum; 
        public int maxFloorNum;
        public int maxNodePerRoute;

        //Singleton Instance
        private static MapDataManager instance;
        public static MapDataManager Instance
        {
            get
            {
                return instance;
            }
        }

        #region Map Object Properties

        //각 전시실이 위치하는 층을 저장
        private List<int> roomFloor;
        public List<int> RoomFloor
        {
            get
            {
                return roomFloor;
            }
        }

        //아이템 소환 포인트를 저장
        private List<Transform> itemGenerationPoints;
        public List<Transform> ItemGenerationPoints
        {
            get
            {
                return itemGenerationPoints;
            }
        }

        //전시실 내부 순환경로를 저장
        private List<Route> inRoomRoutes;
        public List<Route> InRoomRoutes
        {
            get
            {
                return inRoomRoutes;
            }
        }

        //방과 방 사이 경로를 저장
        private List<List<Route>> roomToRoomRoutes;
        public List<List<Route>> RoomToRoomRoutes
        {
            get
            {
                return roomToRoomRoutes;
            }
        }

        //계단->방 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        private List<List<Route>> stairToRoomRoutes;
        public List<List<Route>> StairToRoomRoutes
        {
            get
            {
                return stairToRoomRoutes;
            }
        }

        //방->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        private List<List<Route>> roomToStairRoutes;
        public List<List<Route>> RoomToStairRoutes
        {
            get
            {
                return roomToStairRoutes;
            }
        }

        //계단->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1 
        private List<List<Route>> stairToStairRoutes;
        public List<List<Route>> StairToStairRoutes
        {
            get
            {
                return stairToStairRoutes;
            }
        }

        //현존하는 모든 경로 저장(NPC의 초기위치 설정을 위해)
        private List<Route> allGenrationRoutes;
        private List<RouteNode> allGenerationPoints;
        //public List<RouteNode> AllGenerationPoints

        #endregion

        private void Awake()
        {
            
        }

        private void Start()
        {
            //In-Room 루트와 Room-to-Room 루트를 각각 리스트화한다.
            roomFloor = new List<int>();
            itemGenerationPoints = new List<Transform>();
            inRoomRoutes = new List<Route>();
            roomToRoomRoutes = new List<List<Route>>();
            stairToRoomRoutes = new List<List<Route>>();
            roomToStairRoutes = new List<List<Route>>();
            stairToStairRoutes = new List<List<Route>>();
            allGenrationRoutes = new List<Route>();
            allGenerationPoints = new List<RouteNode>();

            for (int i = 0; i < maxRoomNum; i++)
            {
                roomFloor.Add(-1);
                inRoomRoutes.Add(null);

                List<Route> tempRouteSet = new List<Route>();
                for (int j = 0; j < maxRoomNum; j++)
                    tempRouteSet.Add(null);
                roomToRoomRoutes.Add(tempRouteSet);

                tempRouteSet = new List<Route>();
                List<Route> tempRouteSet2 = new List<Route>();

                //방->계단과 계단->방 루트 2가지에 각각 상층, 하층 루트 2가지씩 배정
                for (int j = 0; j < 2; j++)
                {
                    tempRouteSet.Add(null);
                    tempRouteSet2.Add(null);
                }
                stairToRoomRoutes.Add(tempRouteSet);
                roomToStairRoutes.Add(tempRouteSet2);
            }

            for (int i = 0; i < maxFloorNum; i++)
            {
                List<Route> tempRouteSet = new List<Route>();
                for (int j = 0; j < 4; j++)
                {
                    tempRouteSet.Add(null);
                }
                stairToStairRoutes.Add(tempRouteSet);
            }

            GameObject roomsRoot
                = transform.Find("Exhibit Rooms").gameObject;
            ExhibitRoom[] roomsArray = roomsRoot.GetComponentsInChildren<ExhibitRoom>();
            foreach (ExhibitRoom room in roomsArray)
            {
                roomFloor[room.num] = room.floor;

                Transform[] tempItemGenPointArray = room.gameObject.GetComponentsInChildren<Transform>();
                for (int i = 1; i < tempItemGenPointArray.Length; i++)
                    itemGenerationPoints.Add(tempItemGenPointArray[i]);
            }

            GameObject inRoomRoutesRoot
                = transform.Find("In-Room Routes").gameObject;
            Route[] inRoomRoutesArray = inRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in inRoomRoutesArray)
            {
                inRoomRoutes[route.curRoom] = route;
            }
           

            GameObject roomToRoomRoutesRoot
                = transform.Find("Room-to-Room Routes").gameObject;
            Route[] roomToRoomRoutesArray = roomToRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in roomToRoomRoutesArray)
            {
                roomToRoomRoutes[route.startRoom][route.endRoom] = route;
            }

            GameObject stairToRoomRoutesRoot
                = transform.Find("Stair-to-Room Routes").gameObject;
            Route[] stairToRoomRoutesArray = stairToRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in stairToRoomRoutesArray)
            {
                if (route.stairType == Route.StairType.down)
                    stairToRoomRoutes[route.endRoom][0] = route;
                else
                    stairToRoomRoutes[route.endRoom][1] = route;
            }

            GameObject roomToStairRoutesRoot
                = transform.Find("Room-to-Stair Routes").gameObject;
            Route[] roomToStairRoutesArray = roomToStairRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in roomToStairRoutesArray)
            {
                if (route.stairType == Route.StairType.down)
                    roomToStairRoutes[route.startRoom][0] = route;
                else
                    roomToStairRoutes[route.startRoom][1] = route;
            }

            GameObject stairToStairRoutesRoot
                = transform.Find("Stair-to-Stair Routes").gameObject;
            Route[] stairToStairRouteArray = stairToStairRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in stairToStairRouteArray)
            {
                if (route.stairSide == Route.StairSide.left && route.stairType == Route.StairType.down)
                    stairToStairRoutes[route.floor][0] = route;
                else if (route.stairSide == Route.StairSide.left && route.stairType == Route.StairType.up)
                    stairToStairRoutes[route.floor][1] = route;
                else if (route.stairSide == Route.StairSide.right && route.stairType == Route.StairType.down)
                    stairToStairRoutes[route.floor][2] = route;
                else if (route.stairSide == Route.StairSide.right && route.stairType == Route.StairType.up)
                    stairToStairRoutes[route.floor][3] = route;
            }
            
            Route[] allRouteArray = GetComponentsInChildren<Route>();
            for (int i = 0; i < allRouteArray.Length; i++)
            {
                if (allRouteArray[i].routeType != Route.RouteType.Stair_to_Stair
                    && allRouteArray[i].routeType != Route.RouteType.Room_to_Stair)
                {
                    allGenrationRoutes.Add(allRouteArray[i]);
                    for (int j = 1; j < allRouteArray[i].NodeSet.Length - 1; j++)
                    {
                        allGenerationPoints.Add(allRouteArray[i].NodeSet[j]);
                    }
                }
            }
            ifRouteAssigned = new bool[allGenrationRoutes.Count];
            ifPointAssigned = new bool[allGenerationPoints.Count];

            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");
        }

        #region Public Methods

        bool[] ifRouteAssigned;
        int assignedRouteNum = 0;

        public Route getRandomRoute()
        {
            if (assignedRouteNum >= allGenrationRoutes.Count)  //All available routes are assinged
                return null;                

            int r;
            do
            {
                r = Random.Range(0, allGenrationRoutes.Count);
            } while (ifRouteAssigned[r]);

            ifRouteAssigned[r] = true;
            assignedRouteNum += 1;

            return allGenrationRoutes[r];
        }

        bool[] ifPointAssigned;
        int assignedPointNum = 0;
        List<Vector3> assinedLocations = new List<Vector3>();
        public RouteNode GetRandomGenerationPoint()
        {
            if (assignedPointNum >= allGenerationPoints.Count)  //All available points are assinged
                return null;

            int r;
            do
            {
                r = Random.Range(0, allGenerationPoints.Count);
                Vector3 newLoc = allGenerationPoints[r].transform.position;
                if (assinedLocations.FindAll(loc => loc.Equals(newLoc)).Count != 0)
                    ifPointAssigned[r] = true;
                else
                    assinedLocations.Add(newLoc);
            } while (ifPointAssigned[r]);

            ifPointAssigned[r] = true;
            assignedPointNum += 1;
            return allGenerationPoints[r];
        }

        #endregion
    }
}
