using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : CharController
{ 
    public Route curRoute;
    private RouteNode[] routeNodeSet;
    private int curNodeNum;

    RoutingManager routingManager;

    GameObject roomToRoomRoutesRoot;
    GameObject inRoomRoutesRoot;

    private new void Awake()
    {
        base.Awake();   // Raycast box initiallize

        routingManager = RoutingManager.Instance;
    }

    // Use this for initialization
    void Start()
    {
        if (curRoute != null)
        {
            routeNodeSet = curRoute.NodeSet;
            curNodeNum = 0;
            transform.position = routeNodeSet[curNodeNum].transform.position;
        }

        StartCoroutine("MoveCheck");
    }

    //int moveLock = 0;

    bool blocked = false;
    int itemWatchingTime = 0;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log(targetPoint);
        if (!blocked)
        {
            Move();
            RouteEndCheck();
        }
    }

    protected new void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        blocked = true;
    }

    #region Routing

    Vector2 direction = new Vector2(0, 0);
    protected override IEnumerator MoveCheck()
    {
        while (true)
        {
            if (itemWatchingTime > 0)
            {
                itemWatchingTime -= 1;
                yield return new WaitForSeconds(1.0f / moveSpeed);
                continue;
            }

            if (blocked == true)
                blocked = false;

            /*if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].position) < 0.01f
                && curNodeNum < routeNodeSet.Length)
            {
                transform.position = routeNodeSet[curNodeNum + 1].position;
                curNodeNum++;
            }*/

            startPoint = (Vector2)transform.position;   // Set starting point
            if (curNodeNum < routeNodeSet.Length)
                direction = ((Vector2)(routeNodeSet[curNodeNum + 1].transform.position - transform.position)).normalized;
            else
                direction = new Vector2(0, 0);
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
                    Debug.Log(hit.transform.gameObject.name);
                    break;
                }
            }
            if (ifHit)
                blocked = true;

            yield return new WaitForSeconds(1.0f / moveSpeed);
        }
    }

    private void RouteEndCheck()
    {
        //Debug.Log(curNodeNum);
        //Debug.Log(routeNodeSet.Length);
        if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].transform.position) < 0.01f)
        {
            transform.position = routeNodeSet[curNodeNum + 1].transform.position;

            if (curNodeNum < routeNodeSet.Length - 2)
            {
                curNodeNum += 1;
                if (routeNodeSet[curNodeNum].ifItemPoint)
                {
                    itemWatchingTime = Random.Range(3, 12);
                }
            }
            else
            {
                Debug.Log("Route set");

                if (curRoute.routeType == Route.RouteType.In_Room)
                {
                    Debug.Log("Case 1");

                    curRoute = routingManager.RoomToRoomRouteSet[curRoute.curRoom][1 - curRoute.curRoom];
                }
                else
                {
                    Debug.Log("Case 2");

                    Debug.Log(curRoute.endRoom);
                    curRoute = routingManager.InRoomRouteSet[curRoute.endRoom];
                }

                routeNodeSet = curRoute.NodeSet;
                curNodeNum = 0;
            }
        }
    }

    /*private void OnTriggerEnter2D(Collider2D collision)
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
    }*/

    #endregion

}
