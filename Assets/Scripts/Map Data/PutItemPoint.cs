using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(PhotonView))]
    public class PutItemPoint : Photon.PunBehaviour
    {
        public bool Activated = false;
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (Activated == false) return;

            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem != null)
            {
                UIManager.Instance.SetPutItemPopUp(this);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (Activated == false) return;

            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem != null)
            {
                UIManager.Instance.RemovePutItemPopUp();
            }
        }
    }
}
