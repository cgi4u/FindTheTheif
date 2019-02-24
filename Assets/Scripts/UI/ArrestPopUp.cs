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
            Debug.Log("Try to arrest.");
            if (selectedThief != null)
            {
                MultiplayRoomManager.Instance.photonView.RPC("ArrestThief", PhotonTargets.AllViaServer, selectedThief.GetComponent<PhotonView>().ownerId);
                selectedThief.GetComponent<PhotonView>().RequestOwnership();
            }
            else
                MultiplayRoomManager.Instance.photonView.RPC("ArrestFailed", PhotonTargets.All, PhotonNetwork.player.ID);

            GetComponent<RectTransform>().anchoredPosition = orgAnchorPos;
            gameObject.SetActive(false);
        }
    }
}
