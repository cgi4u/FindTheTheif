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

        PlayerController player;
        private void Awake()
        {
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

        #region Methods for Detective Skills

        float oldSpeed;
        [SerializeField]
        float boostSpeed;
        public void SpeedBoost()
        {
            oldSpeed = player.MoveSpeed;
            player.MoveSpeed = boostSpeed;
            StartCoroutine("ReturnSpeed");
        }

        [SerializeField]
        float speedBoostTime;
        private IEnumerator ReturnSpeed()
        {
            yield return new WaitForSeconds(speedBoostTime);
            player.MoveSpeed = oldSpeed;
        }

        #endregion  
    }
}
