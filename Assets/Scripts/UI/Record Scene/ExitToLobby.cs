using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;

public class ExitToLobby : MonoBehaviour
{
    public string loginSceneName;
    public string lobbySceneName;

    public void OnExit()
    {
        if (PhotonNetwork.connected)
        {
            PhotonNetwork.LeaveRoom();
            SceneManager.LoadScene(lobbySceneName);
        }
        else
            SceneManager.LoadScene(loginSceneName);
    }
}
