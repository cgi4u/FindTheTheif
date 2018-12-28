using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutingManager : MonoBehaviour {
    public int maxRoomNum; //나중에 const변경 필요

    private static RoutingManager instance;
    public static RoutingManager Instance
    {
        get
        {
            return instance;
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

        for (int i = 0; i < maxRoomNum; i++)
        {
            inRoomRouteSet.Add(null);

            List<Route> tempRouteSet = new List<Route>();
            for (int j = 0; j < maxRoomNum; j++)
                tempRouteSet.Add(null);
            roomToRoomRouteSet.Add(tempRouteSet);
        }

        GameObject inRoomRoutesRoot
            = transform.Find("In-Room Routes").gameObject;
        Route[] inRoomRouteArray = inRoomRoutesRoot.GetComponentsInChildren<Route>();
        foreach (Route route in inRoomRouteArray)
        {
            Debug.Log(route.curRoom);
            inRoomRouteSet[route.curRoom] = route;
            Debug.Log(inRoomRouteSet[route.curRoom].curRoom);
        }
        Debug.Log(inRoomRouteSet.Count);

        GameObject roomToRoomRoutesRoot
            = transform.Find("Room-to-Room Routes").gameObject;
        Route[] roomToRoomRouteArray = roomToRoomRoutesRoot.GetComponentsInChildren<Route>();
        foreach (Route route in roomToRoomRouteArray)
        {
            roomToRoomRouteSet[route.startRoom][route.endRoom] = route;
        }
        Debug.Log(roomToRoomRouteSet.Count);
    }
}
