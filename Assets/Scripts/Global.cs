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

    public static class GlobalFunctions
    {
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

        public static PhotonPlayer GetPlayerByID(int id)
        {
            if (!PhotonNetwork.inRoom)
            {
                Debug.LogError("GetPlayerByID must be called in a Photon game room.");
                return null;
            }

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if (player.ID == id)
                    return player;
            }

            return null;
        }
    }
}