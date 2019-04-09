using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheThief
{
    public static class PhotonExtends
    {
        public static void SetRoomCustomProp(object key, object value)
        {
            if (!PhotonNetwork.inRoom)
            {
                Debug.LogError("SetRoomCustomPropsByElem must be called in a Photon game room.");
                return;
            }

            Hashtable roomCp = PhotonNetwork.room.CustomProperties;
            roomCp[key] = value;
            PhotonNetwork.room.SetCustomProperties(roomCp);
        }

        public static void SetLocalPlayerProp(object key, object value)
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("SetLocalPlayerPropsByElem must be called in Photon server.");
                return;
            }

            Hashtable playerCp = PhotonNetwork.player.CustomProperties;
            playerCp[key] = value;
            PhotonNetwork.player.SetCustomProperties(playerCp);
        }

        public static void Destroy(GameObject gameObject)
        {
            if (PhotonNetwork.connected)
                PhotonNetwork.Destroy(gameObject);
            else
                Destroy(gameObject);
        }
    }
}
