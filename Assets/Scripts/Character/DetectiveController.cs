﻿using System.Collections;
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

        public static void ResetPlayerInstances() { playerInstances.Clear(); }

        public static void SetAlertForAllInstances(float seconds)
        {
            foreach (PlayerController player in playerInstances)
                UIManager.Instance.SetAlertDuringSeconds(player, seconds);
        }

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
        }
    }
}
