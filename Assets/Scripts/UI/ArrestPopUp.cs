using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class ArrestPopUp : MonoBehaviour
    {
        private bool isThief;
        private int thiefID;

        private Vector3 orgAnchorPos;
        public Vector3 OrgAnchorPos
        {
            get
            {
                return orgAnchorPos;
            }
        }

        private void Awake()
        {
            orgAnchorPos = GetComponent<RectTransform>().anchoredPosition;
        }

        public void Set(bool _isTheif, int _thiefID)
        {
            isThief = _isTheif;
            if (isThief)
                thiefID = _thiefID;
        }
        
        public void TryArrest()
        {
            Debug.Log("Try to arrest.");
            if (isThief)
                MultiplayRoomManager.Instance.photonView.RPC("ArrestThief", PhotonTargets.All, thiefID);
            else
                MultiplayRoomManager.Instance.photonView.RPC("ArrestFailed", PhotonTargets.All, PhotonNetwork.player.ID);

            GetComponent<RectTransform>().anchoredPosition = orgAnchorPos;
            gameObject.SetActive(false);
        }
    }
}
