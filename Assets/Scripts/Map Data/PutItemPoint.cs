using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// The point to which theives can put their stolen items and get the point.
    /// Randomly activated by the multiplay manager.
    /// </summary>
    public class PutItemPoint : Photon.PunBehaviour
    {
        private static List<PutItemPoint> activatedList = new List<PutItemPoint>();

        public static void ResetActivatedPointList()
        {
            activatedList.Clear();
        }

        public static void SetPointPopup(bool state)
        {
            foreach (PutItemPoint point in activatedList)
            {
                point.pointPopup.SetActive(state);
            }
        }

        public bool activated = false;
        public GameObject pointPopup;

        public void Activate()
        {
            activated = true;
            activatedList.Add(this);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (activated == false) return;

            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem != null)
            {
                UIManager.Instance.SetPutItemPopUp(this);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (activated == false) return;

            ThiefController thiefPlayer = collision.gameObject.GetComponent<ThiefController>();
            if (thiefPlayer != null && thiefPlayer == ThiefController.LocalThief && thiefPlayer.StoleItem != null)
            {
                UIManager.Instance.RemovePutItemPopUp();
            }
        }
    }
}
