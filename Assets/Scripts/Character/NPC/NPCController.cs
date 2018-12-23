﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : CharController {
    public int count = 0;

    private new void Awake()
    {
        base.Awake();   // Raycast box initiallize
    }

    // Use this for initialization
    void Start () {
        StartCoroutine("MoveCheck");
	}

    bool moveLock = false;
	// Update is called once per frame
	void Update () {
        //Debug.Log(targetPoint);
        if (!moveLock)
            Move();
	}

    protected new void OnCollisionStay2D(Collision2D collision)
    {
        base.OnCollisionStay2D(collision);

        Vector3 colObjPos = collision.gameObject.transform.position;
        if (Vector3.Magnitude(colObjPos - transform.position) < 0.95f)
        {
            moveLock = true;
        }
    }

    void OnMouseDown()
    {
        if (PhotonNetwork.connected) {
            photonView.RequestOwnership();
            if (photonView.isMine)
            {
                count += 1;
            }
        }
        //소유권을 이전받는것과 변수의 값이 싱크되는것과는 별개. 이것도 따로 계속 싱크를 해줘야함
    }

    #region Routing

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.name);

        if (collision.gameObject.layer == LayerMask.NameToLayer("Path Node"))
        {
            
            PathNode node = collision.gameObject.GetComponent<PathNode>();
            switch (node.direction)
            {
                case "up":
                    direction = Vector2.up;
                    break;
                case "down":
                    direction = Vector2.down;
                    break;
                case "right":
                    direction = Vector2.right;
                    break;
                case "left":
                    direction = Vector2.left;
                    break;
            }
        }
    }

    Vector2 direction = new Vector2(0, 0);
    protected override IEnumerator MoveCheck()
    {
        while ( true )
        {
            moveLock = false;

            startPoint = (Vector2)transform.position;   // Set starting point
            targetPoint = startPoint + direction;
            

            //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
            //자기자신의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
            RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, targetPoint - startPoint, 1.0f);

            //자기 자신 이외에 충돌 물체가 있다면 이동하지 않는다.
            bool ifHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                {
                    //자신의 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
                    //Debug.Log(hit.collider.gameObject.name);
                    ifHit = true;
                    break;
                }
            }
            if (ifHit)
            {
                targetPoint = (Vector2)transform.position;
            }

            yield return new WaitForSeconds(1.0f / moveSpeed);
        }
    }

    #endregion

}
