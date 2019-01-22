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
        [SerializeField]
        private List<int> roomFloor;
        public List<int> RoomFloor
        {
            get
            {
                return roomFloor;
            }
        }

        //아이템 소환 포인트를 저장
        [SerializeField]
        private List<Transform> itemGenerationPoints;
        public List<Transform> ItemGenerationPoints
        {
            get
            {
                return itemGenerationPoints;
            }
        }

        //전시실 내부 순환경로를 저장
        [SerializeField]
        private List<Route> inRoomRoutes;
        public List<Route> InRoomRoutes
        {
            get
            {
                return inRoomRoutes;
            }
        }

        //방과 방 사이 경로를 저장
        [SerializeField]
        private List<Route> roomToRoomRoutes;
        public List<Route> RoomToRoomRoutes
        {
            get
            {
                return roomToRoomRoutes;
            }
        }

        //계단->방 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        [SerializeField]
        private List<Route> stairToRoomRoutes;
        public List<Route> StairToRoomRoutes
        {
            get
            {
                return stairToRoomRoutes;
            }
        }

        //방->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1
        [SerializeField]
        private List<Route> roomToStairRoutes;
        public List<Route> RoomToStairRoutes
        {
            get
            {
                return roomToStairRoutes;
            }
        }

        //계단->계단 경로를 저장
        //index: 내려가는 계단 0, 올라가는 계단 1 
        [SerializeField]
        private List<Route> stairToStairRoutes;
        public List<Route> StairToStairRoutes
        {
            get
            {
                return stairToStairRoutes;
            }
        }

        //현존하는 모든 경로 중 계단으로 향하지 않는 경로 저장(NPC의 초기위치 설정을 위해)
        private List<Route> allGenerationRoutes;
        private List<RouteNode> allGenerationPoints;

        #endregion

        private void Awake()
        {
            allGenerationRoutes = new List<Route>();
            allGenerationPoints = new List<RouteNode>();

            allGenerationRoutes.AddRange(roomToRoomRoutes);
            allGenerationRoutes.AddRange(stairToRoomRoutes);
            allGenerationRoutes.AddRange(inRoomRoutes);

            foreach (Route route in allGenerationRoutes)
            {
                allGenerationPoints.AddRange(route.NodeSet);
            }

            ifRouteAssigned = new bool[allGenerationRoutes.Count];
            ifPointAssigned = new bool[allGenerationPoints.Count];

            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");
        }

        private void Start()
        {
            /*allGenerationRoutes = new List<Route>();
            allGenerationPoints = new List<RouteNode>();
            
            Route[] allRouteArray = GetComponentsInChildren<Route>();
            for (int i = 0; i < allRouteArray.Length; i++)
            {
                if (allRouteArray[i].routeType != Route.RouteType.Stair_to_Stair
                    && allRouteArray[i].routeType != Route.RouteType.Room_to_Stair)
                {
                    allGenerationRoutes.Add(allRouteArray[i]);
                    for (int j = 1; j < allRouteArray[i].NodeSet.Length - 1; j++)
                    {
                        allGenerationPoints.Add(allRouteArray[i].NodeSet[j]);
                    }
                }
            }
            ifRouteAssigned = new bool[allGenerationRoutes.Count];
            ifPointAssigned = new bool[allGenerationPoints.Count];

            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");*/
        }

        #region Public Methods

        bool[] ifRouteAssigned;
        int assignedRouteNum = 0;

        public Route getRandomRoute()
        {
            if (assignedRouteNum >= allGenerationRoutes.Count)  //All available routes are assinged
                return null;                

            int r;
            do
            {
                r = Random.Range(0, allGenerationRoutes.Count);
            } while (ifRouteAssigned[r]);

            ifRouteAssigned[r] = true;
            assignedRouteNum += 1;

            return allGenerationRoutes[r];
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
