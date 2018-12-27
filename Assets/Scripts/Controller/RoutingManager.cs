using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoutingManager : MonoBehaviour { 
    public GameObject roomToRoomRoutesRoot;
    public GameObject inRoomRoutesRoot;

    private static RoutingManager instance;
    public static RoutingManager Instance
    {
        get
        {
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Debug.Log("Error: Multiple instantiation of the routing manager.");
    }
}
