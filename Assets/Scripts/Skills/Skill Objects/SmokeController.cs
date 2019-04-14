using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class SmokeController : Photon.PunBehaviour
    {
        public void Init(float aliveSeconds)
        {
            photonView.RPC("CheckIfThePlayerIncluded", PhotonTargets.Others, true);
            StartCoroutine(DestroyAfterSeconds(aliveSeconds));
        }

        private IEnumerator DestroyAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);

            photonView.RPC("CheckIfThePlayerIncluded", PhotonTargets.Others, false);
            PhotonNetwork.Destroy(gameObject);
        } 

        [PunRPC]
        private void CheckIfThePlayerIncluded(bool state)
        {
            Vector2 effectSize = GetComponent<BoxCollider2D>().size;
            RaycastHit2D[] hits = Physics2D.BoxCastAll(transform.position, effectSize, 0, new Vector2(0f, 0f));
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject == PlayerController.LocalPlayer.gameObject)
                {
                    if (state)
                        UIManager.Instance.ActivateSmokeScreen();
                    else
                        UIManager.Instance.DeActivateSmokeScreen();
                    break;
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (MultiplayRoomManager.Instance.MyTeam == ETeam.Thief
                || PlayerController.LocalPlayer.gameObject != collision.gameObject)
                return;

            UIManager.Instance.ActivateSmokeScreen();

            SpriteRenderer[] smokeSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in smokeSpriteRenderers)
            {
                renderer.enabled = false;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (MultiplayRoomManager.Instance.MyTeam == ETeam.Thief
                || PlayerController.LocalPlayer.gameObject != collision.gameObject)
                return;

            UIManager.Instance.DeActivateSmokeScreen();

            SpriteRenderer[] smokeSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in smokeSpriteRenderers)
            {
                renderer.enabled = true;
            }
        }
    }
}
