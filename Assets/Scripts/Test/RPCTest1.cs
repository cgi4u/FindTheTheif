using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class RPCTest1 : Photon.PunBehaviour
    {
        public void OnCheckButton()
        {
            photonView.RPC("Check", PhotonTargets.All);
        }

        int checkedNum = 0;
        [PunRPC]
        private void Check()
        {
            checkedNum += 1;
            UIManager.Instance.RenewErrorLabel(checkedNum.ToString());
        }
    }
}
