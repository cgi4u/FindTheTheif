using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public enum ETeam { Undefined, Detective, Thief }
    public enum EMoveDirection { Stop, Down, Right, Up, Left }
    public enum EGameState { None, Team_Set, Initialized, Ready, ReadyChecked, Preparing, Started, Game_Set };

    #region Route Properties Enum

    public enum ERouteType { In_Room, Room_to_Room, Room_to_Stair, Stair_to_Room, Stair_to_Stair }
    public enum EStairType { Up, Down, None }
    public enum EStairSide { Left, Right, None }

    #endregion

    #region Item Properties

    public static class ItemProperties
    {
        public static readonly int ItemPropTypeNum = 3;
        public static readonly int ItemPropNumPerType = 3;

        public enum EItemColor { Red, Blue, Yellow }
        public static Color UIColorForProp(EItemColor color)
        {
            switch (color)
            {
                case EItemColor.Red:
                    return new Color(0.67f, 0.2f, 0.2f);
                case EItemColor.Blue:
                    return new Color(0.25f, 0.25f, 0.45f);
                case EItemColor.Yellow:
                    return new Color(0.91f, 0.76f, 0.08f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public enum EItemAge { Ancient, Middle, Modern }
        public static Color UIColorForProp(EItemAge age)
        {
            switch (age)
            {
                case EItemAge.Ancient:
                    return new Color(0.7f, 0.62f, 0.36f);
                case EItemAge.Middle:
                    return new Color(0.15f, 0.74f, 0.24f);
                case EItemAge.Modern:
                    return new Color(0.64f, 0.65f, 0.63f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public enum EItemUsage { Art, Daily, War }
        public static Color UIColorForProp(EItemUsage usage)
        {
            switch (usage)
            {
                case EItemUsage.Art:
                    return new Color(0.91f, 0.47f, 0.12f);
                case EItemUsage.Daily:
                    return new Color(0.29f, 0.71f, 0.89f);
                case EItemUsage.War:
                    return new Color(0.35f, 0.35f, 0.35f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public struct ItemPropStrings
        {
            public string ColorString;
            public string AgeString;
            public string UsageString;
        }
    }

    #endregion


    public static class Constants
    {
        //For sending the number of NPCs using custom room property from lobby to in-game.
        //Should be used in the test version only.
        public static readonly string NPCNumKey = "NPC Number";

        public static readonly int maxSkillNum = 2;
    }

    public static class GlobalFunctions
    {
        public static float GetAngle(Vector2 a, Vector2 b)
        {
            return Mathf.Atan2(a.y - b.y, a.x - b.x) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Randomly shuffle elements of 1-dimensional array. 
        /// </summary>
        public static void RandomizeArray<T>(T[] arr)
        {
            for (int i = 0; i < arr.Length * 3; i++)
            {
                int r1 = Random.Range(0, arr.Length);
                int r2 = Random.Range(0, arr.Length);

                T temp = arr[r1];
                arr[r1] = arr[r2];
                arr[r2] = temp;
            }
        }

        /// <summary>
        /// Randomly shuffle elements of 1-dimensional list. 
        /// </summary>
        public static void RandomizeList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count * 3; i++)
            {
                int r1 = Random.Range(0, list.Count);
                int r2 = Random.Range(0, list.Count);

                T temp = list[r1];
                list[r1] = list[r2];
                list[r2] = temp;
            }
        }
    }
}