using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaster : MonoBehaviour
{
    public void ChangeMasterClient()
    {
        if (PhotonNetwork.isMasterClient)
            PhotonNetwork.SetMasterClient(PhotonNetwork.player.GetNext());
    }
}
