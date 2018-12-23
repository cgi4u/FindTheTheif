using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharController : Photon.PunBehaviour
{

    #region Shared Method(by Players and NPCs)

    protected Vector2 raycastBox; // Collider of a character
    protected void Awake()
    {
        raycastBox = GetComponent<BoxCollider2D>().size - new Vector2(0.05f, 0.05f);   // To ignore collisions on edges
    }

    protected Vector2 startPoint;
    protected Vector2 targetPoint;
    public float moveSpeed;

    protected void Move()
    {
        //설정 속도에 따라 움직일 위치를 계산(MoveTowards) 이후 이동
        Vector2 nextPos = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
        transform.position = (Vector3)nextPos;

        return;
    }

    protected abstract IEnumerator MoveCheck();

    //충돌처리를 위한 부분, 미완성
    protected void OnCollisionStay2D(Collision2D collision)
    {
        Vector3 colObjPos = collision.gameObject.transform.position;
        if (Vector3.Magnitude(colObjPos - transform.position) < 0.95f)
        {
            transform.position = startPoint;
        }
    }

    #endregion
}
