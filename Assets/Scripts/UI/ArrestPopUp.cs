using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(RectTransform))]
    public class ArrestPopUp : MonoBehaviour
    {
        private ThiefController selectedThief;

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

        public void Set(ThiefController _selectedThief)
        {
            selectedThief = _selectedThief;
        }
        
        public void OnArrestButton()
        {
            int thiefID = -1;
            if (selectedThief != null)
                thiefID = selectedThief.photonView.ownerId;

            MultiplayRoomManager.Instance.photonView.RPC("TryToArrest", PhotonTargets.AllViaServer, PhotonNetwork.player.ID, thiefID);

            GetComponent<RectTransform>().anchoredPosition = orgAnchorPos;
            gameObject.SetActive(false);
        }
    }
}
