using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public enum Team { Undefined, Detective, Thief }
    public enum Direction { None, Up, Down, Right, Left }

    public enum RouteType { In_Room, Room_to_Room, Room_to_Stair, Stair_to_Room, Stair_to_Stair }
    public enum StairType { Up, Down, None }
    public enum StairSide { Left, Right, None }

    //Item properties
    public enum ItemColor { Red, Blue, Yellow }
    public enum ItemAge { Ancient, Middle, Modern }
    public enum ItemUsage { Art, Daily, War }
}