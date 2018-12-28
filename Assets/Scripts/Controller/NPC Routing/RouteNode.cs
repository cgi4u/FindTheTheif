using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RouteNode: MonoBehaviour {
    public enum Direction { up, down, left, right };
    
    public bool ifItemPoint;
    public Direction itemDir;
}
