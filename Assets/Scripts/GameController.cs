using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameController: MonoBehaviour {
    #region Public Properties

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    /*
    [Tooltip("The singleton of the game controller")]
    public static GameController gameController;
    */
    public Text teamLabel;

    #endregion

    #region Private Properties

    Team MyTeam;

    #endregion  


    #region Unity Callbacks

    // Use this for initialization
    void Start () {
        //Instantiate the local player(my player)
        Hashtable cp = PhotonNetwork.player.CustomProperties;
        Debug.Log(cp["Test"]);

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Room Controller'", this);
        }
        else
        {
            //Determine team of the local player(For test. At release, it should be determined randomly.)
            if (PhotonNetwork.room.PlayerCount % 2 == 0)
            {
                MyTeam = Team.detective;
                teamLabel.text = "탐정";
            }
            else
            {
                MyTeam = Team.theif;
                teamLabel.text = "도둑";
            }

            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(.5f, .5f, 0f), Quaternion.identity, 0);
            localPlayer.GetComponent<PlayerController>().SetTeam(MyTeam);
        }
    }
	
	// Update is called once per frame
	void Update () {
        
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
