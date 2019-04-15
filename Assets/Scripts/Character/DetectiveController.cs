using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(PlayerController))]
    public class DetectiveController : Photon.PunBehaviour
    {
        private static DetectiveController localDetective;
        public static DetectiveController LocalDetective
        {
            get
            {
                return localDetective;
            }
        }

        private static List<PlayerController> playerInstances = new List<PlayerController>();
        public static List<PlayerController> PlayerInstances
        {
            get
            {
                return playerInstances;
            }
        }
        private bool sensingMode = false;
        private bool sensingAlretIsSet = false;

        PlayerController player;
        private void Awake()
        {
            playerInstances.Add(GetComponent<PlayerController>());

            if (!photonView.isMine)
                return;

            if (localDetective != null)
            {
                Debug.LogError("Multiple instantiation of local detective.");
                PhotonExtends.Destroy(gameObject);
                return;
            }

            localDetective = this;
            player = GetComponent<PlayerController>();
        }

        /*
        private void Update()
        {
            
            if (sensingMode)
            {
                UIManager.Instance.SetSensingAlert(transform.position - ThiefController.LocalThief.transform.position);
            }
            
        }

        public void SetSensingDuringSeconds(float seconds)
        {
            sensingMode = true;
            StartCoroutine(EndSensingAfterSeconds(seconds));
        }

        private IEnumerator EndSensingAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            sensingMode = false;
            UIManager.Instance.DeactiveSensingAlert();
        }
        */
    }
}
