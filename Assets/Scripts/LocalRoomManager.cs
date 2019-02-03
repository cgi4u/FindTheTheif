using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheTheif
{
    public class LocalRoomManager: Photon.PunBehaviour
    {
        #region Public Properties

        [Tooltip("The prefab to use for representing the player")]
        public GameObject playerPrefab;
        /*
        [Tooltip("The singleton of the game controller")]
        public static GameController gameController;
        */
        public Text teamLabel;

        [Tooltip("The prefab of the room controller")]
        public GameObject multiplayRoomManagerPrefab;

        #endregion

        #region Private Properties

        Team MyTeam;

        #endregion

        #region Unity Callbacks

        //TODO: 마스터 클라이언트일 경우, 멀티플레이 룸컨트롤러 생성

        // Use this for initialization
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

        #endregion

        #region Public Methods

        public void ExitRoom()
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene("Lobby");
        }

        #endregion

    }
}
