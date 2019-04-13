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

        [SerializeField]
        SecretPathController linkedPath = null;
        public void Link(SecretPathController path)
        {
            linkedPath = path;
        }

        [SerializeField]
        bool active = true;
        /*public void OnTriggerEnter2D(Collision2D collision)
        
            Debug.Log("Triggered");

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
        }*/

        public Vector3 GetLinkedPos()
        {
            if (linkedPath == null)
                return new Vector3(0f, 0f, -1f);

            RaycastHit2D[] hits = Physics2D.BoxCastAll(linkedPath.transform.position, new Vector2(1f, 1f), 0, new Vector2(0f, 0f));
            foreach (RaycastHit2D hit in hits)
            {
                if (!hit.collider.isTrigger)
                    return new Vector3(0f, 0f, -1f);
            }

            return linkedPath.transform.position;
        }
    }
}
