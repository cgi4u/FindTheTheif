using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public abstract class CharController : Photon.PunBehaviour
    {

        #region Shared Method(by Players and NPCs)

        protected Vector2 raycastBox; // Collider of a characters
        //protected static readonly Vector2 spriteOffset = new Vector2(0f, -0.15f); // Offset for character sprites
        protected void Awake()
        {
            raycastBox = GetComponent<BoxCollider2D>().size;   // To ignore collisions on edges
        }

        protected Vector2 startPoint;
        protected Vector2 targetPoint;
        public float moveSpeed;

        protected void Move()
        {
            //설정 속도에 따라 움직일 위치를 계산(MoveTowards) 이후 이동
            Vector2 nextPos = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            transform.position = (Vector3)nextPos;

            //이동 위치에 따라 스프라이트 우선순위를 결정(y축 위치가 더 큰 캐릭터가 뒤로 가도록)
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);

            return;
        }

        protected abstract IEnumerator MoveCheck();

        //충돌처리를 위한 부분, 미완성
        protected void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log(collision.gameObject.name);

            transform.position = startPoint;
        }

        #endregion
    }
}
