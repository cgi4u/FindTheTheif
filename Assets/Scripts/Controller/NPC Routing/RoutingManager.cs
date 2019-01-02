using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class RoutingManager : MonoBehaviour
    {
        public int maxRoomNum; //나중에 const변경 필요

        private static RoutingManager instance;
        public static RoutingManager Instance
        {
            get
            {
                return instance;
            }
        }

           
        private List<int> roomFloor;
        public List<int> RoomFloor
        {
            get
            {
                return roomFloor;
            }
        }

        private List<Route> inRoomRouteSet;
        public List<Route> InRoomRouteSet
        {
            get
            {
                return inRoomRouteSet;
            }
        }

        private List<List<Route>> roomToRoomRouteSet;
        public List<List<Route>> RoomToRoomRouteSet
        {
            get
            {
                return roomToRoomRouteSet;
            }
        }

        private List<List<Route>> stairToRoomRouteSet;
        public List<List<Route>> StairToRoomRouteSet
        {
            get
            {
                return stairToRoomRouteSet;
            }
        }

        private List<List<Route>> roomToStairRouteSet;
        public List<List<Route>> RoomToStairRouteSet
        {
            get
            {
                return roomToStairRouteSet;
            }
        }

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
            inRoomRouteSet = new List<Route>();
            roomToRoomRouteSet = new List<List<Route>>();
            roomFloor = new List<int>();

            for (int i = 0; i < maxRoomNum; i++)
            {
                roomFloor.Add(-1);
                inRoomRouteSet.Add(null);

                List<Route> tempRouteSet = new List<Route>();
                for (int j = 0; j < maxRoomNum; j++)
                    tempRouteSet.Add(null);
                roomToRoomRouteSet.Add(tempRouteSet);
            }

            GameObject roomsRoute
                = transform.Find("Exhibit Rooms").gameObject;
            ExhibitRoom[] roomsRouteArray = roomsRoute.GetComponentsInChildren<ExhibitRoom>();
            foreach (ExhibitRoom room in roomsRouteArray)
            {
                roomFloor[room.num] = room.floor;
            }

            GameObject inRoomRoutesRoot
                = transform.Find("In-Room Routes").gameObject;
            Route[] inRoomRoutesArray = inRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in inRoomRoutesArray)
            {
                Debug.Log(route.curRoom);
                inRoomRouteSet[route.curRoom] = route;
                Debug.Log(inRoomRouteSet[route.curRoom].curRoom);
            }
           

            GameObject roomToRoomRoutesRoot
                = transform.Find("Room-to-Room Routes").gameObject;
            Route[] roomToRoomRoutesArray = roomToRoomRoutesRoot.GetComponentsInChildren<Route>();
            foreach (Route route in roomToRoomRoutesArray)
            {
                roomToRoomRouteSet[route.startRoom][route.endRoom] = route;
            } 

        }
    }
}
