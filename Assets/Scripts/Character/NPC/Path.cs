using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PathType { In_Room, Room_to_Room }

public class Path : MonoBehaviour {
    public PathType pathType;

    //If room-to-room
    public int startRoom;
    public int endRoom;

    //if in-room
    public int curRoom;
}
