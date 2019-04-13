using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class Smoke : MonoBehaviour
    {
        GameObject localPlayerObj;
        GameObject LocalPlayerObj
        {
            set
            {
                localPlayerObj = value;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            UIManager.Instance.ActivateSmokeScreen();

            SpriteRenderer[] smokeSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in smokeSpriteRenderers)
            {
                
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            UIManager.Instance.DeActivateSmokeScreen();
        }
    }
}
