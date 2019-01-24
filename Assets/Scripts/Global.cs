using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public enum Team { undefined, detective, theif }
    public enum Direction { none, up, down, right, left }

    public enum RouteType { In_Room, Room_to_Room, Room_to_Stair, Stair_to_Room, Stair_to_Stair }
    public enum StairType { up, down }
    public enum StairSide { left, right }

    //Item properties
    public enum ItemColor { Red, Blue, Yellow }
    public enum ItemAge { Ancient, Middle, Modern }
    public enum ItemUsage { Art, Daily, War }
}