using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class SecretPathController : MonoBehaviour
    {
        private void Awake()
        {
            if (MultiplayRoomManager.Instance.MyTeam != ETeam.Thief)
            {
                Debug.LogError("SecretPath should be used by a thief player.");
                Destroy(this);
            }
        }

        SecretPathController linkedPath = null;
        public void Link(SecretPathController path)
        {
            linkedPath = path;
        }

        bool active = true;
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (linkedPath == null || ThiefController.LocalThief == null ||
                ThiefController.LocalThief != collision.gameObject.GetComponent<ThiefController>())
                return;

            if (active)
            {
                linkedPath.active = false;
                collision.gameObject.transform.position = linkedPath.transform.position;
            }
            else
                active = true;
        }
    }
}
