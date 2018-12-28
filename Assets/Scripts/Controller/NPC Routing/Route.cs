using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Route : MonoBehaviour {
    public enum RouteType { In_Room, Room_to_Room }
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
}
