using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameController: Photon.PunBehaviour {
    #region Public Properties

    [Tooltip("The prefab to use for representing the player")]
    public GameObject playerPrefab;
    /*
    [Tooltip("The singleton of the game controller")]
    public static GameController gameController;
    */
    public Text teamLabel;

    public GameObject testNPCPrefab;

    #endregion

    #region Private Properties

    Team MyTeam;

    #endregion  


    #region Unity Callbacks

    //TODO: 마스터 클라이언트일 경우, 룸컨트롤러 생성

    // Use this for initialization
    void Start () {
        //Instantiate the local player(my player)
        Hashtable cp = PhotonNetwork.player.CustomProperties;
        Debug.Log(cp["Team"]);

        if (playerPrefab == null)
        {
            Debug.LogError("<Color=Red><a>Missing</a></Color> playerPrefab Reference. Please set it up in GameObject 'Game Room Controller'", this);
        }
        else if (PhotonNetwork.connected)
        {
            if (cp["Team"] != null)
                MyTeam = (Team)cp["Team"];
            else
                MyTeam = Team.undefined;

            switch (MyTeam)
            {
                case Team.theif: teamLabel.text = "도둑"; break;
                case Team.detective: teamLabel.text = "탐정"; break;
                default: teamLabel.text = "오류"; break;
            }

            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            GameObject localPlayer = PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(.5f, .5f, 0f), Quaternion.identity, 0);
            localPlayer.GetComponent<PlayerController>().SetTeam(MyTeam);

        }

        //GameObject testNPC1 = PhotonNetwork.InstantiateSceneObject(testNPCPrefab.name, new Vector3(0f, 0f, 0f), Quaternion.identity, 0, null);
        //testNPC1.transform.position = new Vector3(0.0f, 0.0f);
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
