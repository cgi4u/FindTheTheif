using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : CharController {
    public GameObject currentRoute;
    private Transform[] routeNodeSet;
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
    void Start () {
        GameObject roomToRoomRoutesRoot = GameObject.Find("Room-to-Room Routes");
        Debug.Log(roomToRoomRoutesRoot.name);
        GameObject inRoomRoutesRoot = GameObject.Find("In-Room Routes");
        Debug.Log(inRoomRoutesRoot.name);

        if (currentRoute != null)
        {
            routeNodeSet = currentRoute.GetComponentsInChildren<Transform>();
            curNodeNum = 1;

            transform.position = routeNodeSet[curNodeNum].position;
        }
        else
        {
            Debug.LogError("Route for NPC " + name + " is not set.");
        }

        StartCoroutine("MoveCheck");
	}

    int moveLock = 0;
	// Update is called once per frame
	void Update () {
        //Debug.Log(targetPoint);
        if (moveLock == 0)
        {
            Move();
            RouteEndCheck();
        }
	}

    protected new void OnCollisionEnter2D(Collision2D collision)
    {
        base.OnCollisionEnter2D(collision);

        moveLock += 1;
    }

    /*void OnMouseDown()
    {
        if (PhotonNetwork.connected) {
            photonView.RequestOwnership();
            if (photonView.isMine)
            {
                count += 1;
            }
        }
        //소유권을 이전받는것과 변수의 값이 싱크되는것과는 별개. 이것도 따로 계속 싱크를 해줘야함
    }*/

    #region Routing

    Vector2 direction = new Vector2(0, 0);
    protected override IEnumerator MoveCheck()
    {
        while ( true )
        {
            if (moveLock > 0)
                moveLock -= 1;

            /*if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].position) < 0.01f
                && curNodeNum < routeNodeSet.Length)
            {
                transform.position = routeNodeSet[curNodeNum + 1].position;
                curNodeNum++;
            }*/

            startPoint = (Vector2)transform.position;   // Set starting point
            if (curNodeNum < routeNodeSet.Length)
                direction = ((Vector2)(routeNodeSet[curNodeNum + 1].position - transform.position)).normalized;
            else
                direction = new Vector2(0, 0);
            targetPoint = startPoint + direction;

            //Debug.Log(routeNodeSet[curNodeNum + 1].position - transform.position);
            //Debug.Log(direction);
            //Debug.Log("Start Point: " + startPoint + " Target Point: " + targetPoint);
            

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
                moveLock += 1;

            yield return new WaitForSeconds(1.0f / moveSpeed);
        }
    }

    private void RouteEndCheck()
    {
        Debug.Log(curNodeNum);
        if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].position) < 0.01f)
        {
            transform.position = routeNodeSet[curNodeNum + 1].position;

            if (curNodeNum < routeNodeSet.Length - 2)
                curNodeNum += 1;
            else
            {
                Debug.Log("Route set");

                if (currentRoute.tag == "In-Room Route")
                {
                    Debug.Log("Case 1");

                    int curRoom = int.Parse(currentRoute.name);
                    int nextRoom = 1 - curRoom;
                    Debug.Log(curRoom + "-" + nextRoom);
                    currentRoute = routingManager.roomToRoomRoutesRoot.transform.Find(curRoom + "-" + nextRoom).gameObject;
                }
                else
                {
                    Debug.Log("Case 2");

                    string curRoom = currentRoute.name.Split('-')[1];
                    Debug.Log(curRoom);
                    currentRoute = routingManager.inRoomRoutesRoot.transform.Find(curRoom).gameObject;
                    Debug.Log(currentRoute);
                }

                routeNodeSet = currentRoute.GetComponentsInChildren<Transform>();
                curNodeNum = 1;
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
