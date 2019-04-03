using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheThief
{
    public class LocalRoomManager: Photon.PunBehaviour
    {
        [Tooltip("The prefab to use for representing the player")]
        public GameObject multiplayRoomManagerPrefab;

        void Start()
        {
            if (PhotonNetwork.player.IsMasterClient)
            {
                if (multiplayRoomManagerPrefab == null)
                {
                    Debug.LogError("Missing multiplayRoomManagerPrefab Reference. Please set it up in GameObject 'Game Controller'", this);
                }
                else
                {
                    PhotonNetwork.InstantiateSceneObject(multiplayRoomManagerPrefab.name, new Vector3(0f, 0f, 10f), Quaternion.identity, 0, null);
                }
            }
        }

        public void ExitRoom()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Lobby");
        }
    }
}
