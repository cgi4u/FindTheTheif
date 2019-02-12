﻿using System.Collections;
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
        public GameObject playerPrefab;
        /*
        [Tooltip("The singleton of the game controller")]
        public static GameController gameController;
        */
        public Text teamLabel;

        [Tooltip("The prefab of the room controller")]
        public GameObject multiplayRoomManagerPrefab;

        Team MyTeam;

        void Start()
        {
            //Inctantiate the room controller(only when the client is the master)
            /*if (PhotonNetwork.player.IsMasterClient)
            {
                if (multiplayRoomManagerPrefab == null)
                {
                    Debug.LogError("Missing multiplayRoomManagerPrefab Reference. Please set it up in GameObject 'Game Controller'", this);
                }
                else
                {
                    PhotonNetwork.InstantiateSceneObject(multiplayRoomManagerPrefab.name, new Vector3(0f, 0f, 10f), Quaternion.identity, 0, null);
                }
            }*/

            //Instantiate the local player(my player)
            if (playerPrefab == null)
            {
                Debug.LogError("Missing playerPrefab Reference. Please set it up in GameObject 'Game Controller'", this);
            }
            else if (PhotonNetwork.connected)
            {
                /*Hashtable playerCp = PhotonNetwork.player.CustomProperties;

                if (playerCp["Team"] != null)
                    MyTeam = (Team)playerCp["Team"];
                else
                    MyTeam = Team.Undefined;

                switch (MyTeam)
                {
                    case Team.Thief: teamLabel.text = "도둑"; break;
                    case Team.Detective: teamLabel.text = "탐정"; break;
                    default: teamLabel.text = "오류"; break;
                }*/

                Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
                // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
                GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0f + PhotonNetwork.player.ID, -5f, 0f), Quaternion.identity, 0);
                localPlayer.GetComponent<PlayerController>().SetTeam(MyTeam);

            }
        }

        public void ExitRoom()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Lobby");
        }
    }
}
