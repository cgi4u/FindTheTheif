using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// Control a NPC's behavior, especially routing
    /// </summary>
    [RequireComponent(typeof(PhotonTransformView), typeof(Animator))]
    public class NPCController : Photon.PunBehaviour, IPunObservable
    {
        #region Intialization

        private Vector2 raycastBox; // Collider of a characters
        private void Awake()
        {
            raycastBox = GetComponent<BoxCollider2D>().size;   // To ignore collisions on edge
            if (PhotonNetwork.connected)
                transform.parent = MultiplayRoomManager.Instance.SceneObjParent;
        }

        [SerializeField]
        public bool Activated = false;

        private MapDataManager mapDataManager;      // Reference of the MapDataManager singleton
        static int maxRandomValue;
        void Start()
        {
            //오프라인 테스트시 수동으로 생성하는 인스턴스에 대한 초기화
            if (mapDataManager == null)
                mapDataManager = MapDataManager.Instance;
        }

        [PunRPC]
        public void Init(int genPointIdx)
        {
            mapDataManager = MapDataManager.Instance;

            RouteNode genPoint = mapDataManager.NPCGenPoints[genPointIdx];
            curRoute = genPoint.gameObject.GetComponentInParent<Route>();

            switch (curRoute.RouteType)
            {
                case RouteType.In_Room:
                    nextRoom = curRoute.CurRoom;
                    break;
                case RouteType.Room_to_Room:
                case RouteType.Stair_to_Room:
                    nextRoom = curRoute.EndRoom;
                    break;
                default:
                    Debug.LogError("Route type error.");
                    break;
            }

            curFloor = mapDataManager.Rooms[nextRoom].Floor;
            for (int i = 0; i < curRoute.NodeSet.Length; i++)
            {
                if (genPoint == curRoute.NodeSet[i])
                {
                    curNodeNum = i;
                    break;
                }
            }

            targetPoint = startPoint = transform.position = (Vector2)curRoute.NodeSet[curNodeNum].DefaultPos;
        }

        #endregion

        private void Update()
        {
            if (!Activated || (PhotonNetwork.connected && !photonView.isMine))
                return;

            if (blockedTime > 0f)
            {
                blockedTime -= Time.deltaTime;
                if (blockedTime <= 0f)
                {
                    blockedTime = 0f;
                }
            }

            if (isMoving)
            {
                Move();
                if ((Vector2)transform.position == targetPoint)
                {  
                    RaycastHit2D[] hits = Physics2D.BoxCastAll(targetPoint, raycastBox, 0, new Vector2(), 0);
                    bool reachedNode = false;
                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit.collider.isTrigger
                            && hit.collider.gameObject == curRoute.NodeSet[curNodeNum + 1].gameObject)
                            reachedNode = true;
                    }

                    if (reachedNode)
                        ChangeNode();
                    else
                        SetNewTargetPoint();
                }
            }
            else if (blockedTime == 0f)
            {
                SetNewTargetPoint();
            }
                
        }

        private void LateUpdate()
        {
            //Set sprite sorting order by using y-axis postion
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        #region NPC Routing

        [SerializeField]
        private bool isMoving = false;
        [SerializeField]
        private float blockedTime = 0f;

        public float moveSpeed;
        private void Move()
        {
            //설정 속도에 따라 움직일 위치를 계산(MoveTowards) 이후 이동
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (PhotonNetwork.connected && !photonView.isMine)
                return;

            if (collision.gameObject.tag == "Player")
            {
                //Debug.Log("Collision with a player name: " + collision.gameObject.GetComponent<PhotonView>().owner.NickName);
                if (Vector2.Distance(collision.gameObject.transform.position, startPoint)
                    >= Vector2.Distance(collision.gameObject.transform.position, targetPoint))
                {
                    //GetComponent<PhotonTransformView>().SetSynchronizedValues(new Vector3(0f, 0f), 0f);
                    isMoving = false;
                    transform.position = targetPoint = startPoint;
                }
                else
                {
                    transform.position = targetPoint;
                }
            }
            else if (collision.gameObject.tag == "NPC")
            {
                if (collision.gameObject.GetInstanceID() > gameObject.GetInstanceID())
                {
                    //GetComponent<PhotonTransformView>().SetSynchronizedValues(new Vector3(0f, 0f), 0f);
                    isMoving = false;
                    transform.position = targetPoint = startPoint;
                }
            }
            else
            {
                Debug.LogError("Collision with undefined object. Object name: " + collision.gameObject.name);
            }

            if (PhotonNetwork.connected)
                photonView.RPC("SetAnimationProperty", PhotonTargets.All, direction, isMoving);
            else
                SetAnimationProperty(direction, isMoving);
        }

        [SerializeField]
        private Vector2 startPoint;
        [SerializeField]
        private Vector2 targetPoint;
        [SerializeField]
        private Vector2 direction = new Vector2(0, 0);
        private Vector2[] directions = new Vector2[4];
        private void SetNewTargetPoint()
        {
            startPoint = targetPoint;

            /*
            // prevent location error can be caused by master client change.
            startPoint.x = Mathf.Round(startPoint.x);
            startPoint.y = Mathf.Round(startPoint.y);
            */

            if (direction == Vector2.down)
            {
                directions[0] = Vector2.down; directions[1] = Vector2.left;
                directions[2] = Vector2.right; directions[3] = Vector2.up;
            }
            else if (direction == Vector2.left)
            {
                directions[0] = Vector2.left; directions[1] = Vector2.up;
                directions[2] = Vector2.down; directions[3] = Vector2.right;
            }
            else if (direction == Vector2.right)
            {
                directions[0] = Vector2.right; directions[1] = Vector2.down;
                directions[2] = Vector2.up; directions[3] = Vector2.left;
            }
            else
            {
                directions[0] = Vector2.up; directions[1] = Vector2.right;
                directions[2] = Vector2.left; directions[3] = Vector2.down;
            }

            //issue
            Vector2 nextNodePos = (Vector2)curRoute.NodeSet[curNodeNum + 1].DefaultPos;
            for (int i = 1; i < directions.Length; i++)
            {
                for (int j = i; j > 0; j--)
                {
                    if (Vector2.Distance(startPoint + directions[j - 1], nextNodePos) >
                        Vector2.Distance(startPoint + directions[j], nextNodePos))
                    {
                        Vector2 temp = directions[j - 1];
                        directions[j - 1] = directions[j];
                        directions[j] = temp;
                    }
                }
            }

            /*
            directions.Sort((x, y)
                => Vector2.Distance(startPoint + x, nextNodePos).CompareTo(Vector2.Distance(startPoint + y, nextNodePos)));

            for (int i = 0; i < directions.Length; i++)
            {
                if (-direction == directions[i])
                {
                    Vector2 temp = directions[i];
                    for (int j = i + 1; j < directions.Length; j++)
                        directions[j - 1] = directions[j];
                    directions[directions.Length - 1] = temp;
                    break;
                }
            }
            */

            Vector2 newDirection = new Vector2();
            for (int i = 0; i < directions.Length; i++)
            {
                // Check if there is a collider in the travel path of this object.
                RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, directions[i], directions[i].magnitude);

                bool ifHit = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                    {
                        ifHit = true;
                        break;
                    }
                }

                if (!ifHit)
                {
                    newDirection = directions[i];
                    break;
                }

                // Check only one main direction in In-Room Route.
                if (curRoute.RouteType == RouteType.In_Room)
                    break;
            }

            if (newDirection == new Vector2())
            {
                isMoving = false;

                if (PhotonNetwork.connected)
                    photonView.RPC("SetAnimationProperty", PhotonTargets.All, direction, isMoving);
                else
                    SetAnimationProperty(direction, isMoving);
            }
            else
            {
                if (direction != newDirection)
                {
                    isMoving = false;
                    blockedTime = Random.Range(0f, 0.3f);

                    if (PhotonNetwork.connected)
                        photonView.RPC("SetAnimationProperty", PhotonTargets.All, direction, isMoving);
                    else
                        SetAnimationProperty(direction, isMoving);
                }
                else
                {
                    isMoving = true;
                    targetPoint = startPoint + newDirection;

                    if (PhotonNetwork.connected)
                        photonView.RPC("SetAnimationProperty", PhotonTargets.All, newDirection, isMoving);
                    else
                        SetAnimationProperty(newDirection, isMoving);
                }

                direction = newDirection;
            }
        }

        [SerializeField]
        private Route curRoute;
        [SerializeField]
        private int curNodeNum;

        [SerializeField]
        private int prevRoom;
        [SerializeField]
        private int nextRoom;
        [SerializeField]
        private int curFloor;
        /// <summary>
        /// Check if this NPC arrived at next node or route end. If did, change target node or current route. 
        /// </summary>
        private void ChangeNode()
        {
            //GetComponent<PhotonTransformView>().SetSynchronizedValues(new Vector3(0f, 0f), 0f);
            curNodeNum += 1;
            if (curRoute.NodeSet[curNodeNum].IsItemPoint)       // Set item watching time.
            {
                isMoving = false;
                blockedTime = Random.Range(1f, 3f);

                if (PhotonNetwork.connected)
                    photonView.RPC("SetAnimationProperty", PhotonTargets.All, curRoute.NodeSet[curNodeNum].ItemDir, isMoving);
                else
                    SetAnimationProperty(curRoute.NodeSet[curNodeNum].ItemDir, isMoving);
            }

            if (curNodeNum == curRoute.NodeSet.Length - 1)       // Route ends, get next route.
            {
                ChangeCurRoute();
            }
        }

        
        void ChangeCurRoute()
        {
            Route nextRoute;
            int randomIdx = -1;
            switch (curRoute.RouteType)
            {
                case RouteType.In_Room:                 // Currunt root is In Room Route -> Next can be Room to Room/Stair
                    prevRoom = curRoute.CurRoom;
                    nextRoom = Random.Range(curRoute.CurRoom + 1, 
                                    curRoute.CurRoom + mapDataManager.Rooms.Count) % mapDataManager.Rooms.Count;

                    if (curFloor == mapDataManager.Rooms[nextRoom].Floor)       // The next room is in the same floor -> Room to Room route
                    {
                        //Debug.Log("Case: Room-to-Room");
                        List<Route> toNextRoomRoutes = mapDataManager.Rooms[prevRoom].ToRoomRoutes[nextRoom];
                        if (toNextRoomRoutes == null)
                        {
                            Debug.LogError("Room " + prevRoom + " to Room " + nextRoom + " Route is not set.");
                            return;
                        }

                        randomIdx = Random.Range(0, toNextRoomRoutes.Count);
                        nextRoute = toNextRoomRoutes[randomIdx];
                    }
                    else        // The next room is in one of the other floors -> Room to Stair route
                    {
                        StairSide stairSide = mapDataManager.Rooms[prevRoom].AdjStairSide;
                        StairType stairType;
                        if (mapDataManager.Rooms[nextRoom].Floor < curFloor)
                            stairType = StairType.Down;
                        else
                            stairType = StairType.Up;

                        Route[] candRouteSet = mapDataManager.Rooms[prevRoom].ToStairRoutes.RoutesWithSideAndType(stairSide, stairType);

                        if (candRouteSet.Length == 0)
                        {
                            Debug.LogError("Room " + prevRoom + " to " + stairSide + " " + stairType + " Stair Route is not set.");
                            return;
                        }

                        randomIdx = Random.Range(0, candRouteSet.Length);
                        nextRoute = candRouteSet[randomIdx];
                    }

                    break;
                case RouteType.Room_to_Stair:              // Current route is To Stair Route -> Next can be Stair to Room/Stair
                case RouteType.Stair_to_Stair:
                    if (curRoute.StairType == StairType.Down)       // Current route ends with down stair
                        curFloor -= 1;
                    else
                        curFloor += 1;
                    
                    if (mapDataManager.Rooms[nextRoom].Floor == curFloor)   // The target room is in this floor -> Stair to Room Route
                    {
                        StairSide stairSide = mapDataManager.Rooms[prevRoom].AdjStairSide;
                        StairType stairType;
                        if (mapDataManager.Rooms[nextRoom].Floor < curFloor)
                            stairType = StairType.Down;
                        else
                            stairType = StairType.Up;


                        Route[] searchRouteSet = mapDataManager.Rooms[nextRoom].FromStairRoutes.RoutesWithSideAndType(stairSide, stairType);
                        List<Route> candRouteSet = new List<Route>();
                        foreach (Route route in searchRouteSet)
                        {
                            if (route.NodeSet[0].DefaultOffset == curRoute.NodeSet[curRoute.NodeSet.Length - 1].DefaultOffset)
                                candRouteSet.Add(route);
                        }

                        if (candRouteSet.Count == 0)
                        {
                            Debug.LogError(stairSide + " " + stairType + " Stair to Room " + nextRoom + " Route is not set.");
                            return;
                        }

                        nextRoute = candRouteSet[Random.Range(0, candRouteSet.Count)];
                        for (int i = 0; i < searchRouteSet.Length; i++)
                        {
                            if (searchRouteSet[i] == nextRoute)
                            {
                                randomIdx = i;
                                break;
                            }
                        }

                        startPoint = targetPoint = transform.position = curRoute.NodeSet[0].DefaultPos;
                    }
                    else   // The target room isn't in this floor -> Stair to Stair Route
                    {
                        StairSide stairSide = mapDataManager.Rooms[prevRoom].AdjStairSide;
                        StairType stairType;
                        if (mapDataManager.Rooms[nextRoom].Floor < curFloor)
                            stairType = StairType.Down;
                        else
                            stairType = StairType.Up;

                        Route[] searchRouteSet = mapDataManager.StairToStairRoutes[curFloor - 1].RoutesWithSideAndType(stairSide, stairType);
                        List<Route> candRouteSet = new List<Route>();
                        foreach (Route route in searchRouteSet)
                        {
                            if (route.NodeSet[0].DefaultOffset == curRoute.NodeSet[curRoute.NodeSet.Length - 1].DefaultOffset)
                                candRouteSet.Add(route);
                        }

                        if (candRouteSet.Count == 0)
                        {
                            Debug.LogError(stairSide + " " + stairType + " Stair to Stair Route in " + curFloor + "th Floor is not set.");
                            return;
                        }

                        nextRoute = candRouteSet[Random.Range(0, candRouteSet.Count)];
                        for (int i = 0; i < searchRouteSet.Length; i++)
                        {
                            if (searchRouteSet[i] == nextRoute)
                            {
                                randomIdx = i;
                                break;
                            }
                        }

                        startPoint = targetPoint = transform.position = nextRoute.NodeSet[0].DefaultPos;
                    }
                    break;
                default:    // The current route is To Room Route -> Next is In Room Route
                    nextRoute = mapDataManager.Rooms[nextRoom].InRoomRoute;

                    if (nextRoute == null)
                    {
                        Debug.LogError("In Room Route of " + nextRoom + "th room is not set.");
                        return;
                    }
                    break;
            }

            if (PhotonNetwork.connected)
                photonView.RPC("SetNextRoute", PhotonTargets.All, nextRoute.RouteType, nextRoute.CurRoom,
                                        nextRoute.StartRoom, nextRoute.EndRoom, nextRoute.StairType, nextRoute.StairSide, randomIdx);
            else
            {
                curRoute = nextRoute;
                curNodeNum = 0;
            }
        }

        [PunRPC]
        void SetNextRoute(RouteType type, int curRoom, int startRoom, int endRoom, StairType stairType, StairSide stairSide, int randomIdx)
        {
            Debug.Log(GetComponent<PhotonView>().instantiationId + " before route: " + curRoute.gameObject.name + ", Prev Room: " + prevRoom + ", Next Room: " + nextRoom);

            curNodeNum = 0;
            switch (type)
            {
                case RouteType.In_Room:
                    curRoute = mapDataManager.Rooms[curRoom].InRoomRoute;
                    break;
                case RouteType.Room_to_Room:
                    curRoute = mapDataManager.Rooms[startRoom].ToRoomRoutes[endRoom][randomIdx];
                    break;
                case RouteType.Room_to_Stair:
                    curRoute = mapDataManager.Rooms[startRoom].ToStairRoutes.RoutesWithSideAndType(stairSide, stairType)[randomIdx];
                    break;
                case RouteType.Stair_to_Room:
                    curRoute = mapDataManager.Rooms[endRoom].FromStairRoutes.RoutesWithSideAndType(stairSide, stairType)[randomIdx];
                    break;
                case RouteType.Stair_to_Stair:
                    curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].RoutesWithSideAndType(stairSide, stairType)[randomIdx];
                    break;
            }

            Debug.Log(GetComponent<PhotonView>().instantiationId + " after route: " + curRoute.gameObject.name);
        }

        [PunRPC]
        private void SetAnimationProperty(Vector2 direction, bool isMoving)
        {
            Animator animator = GetComponent<Animator>();

            int directionInt = -1;
            if (direction == Vector2.up)
                directionInt = 0;
            else if (direction == Vector2.down)
                directionInt = 1;
            else if (direction == Vector2.left)
                directionInt = 2;
            else if (direction == Vector2.right)
                directionInt = 3;

            if (animator.GetInteger("Direction") != directionInt)
                animator.SetInteger("Direction", directionInt);

            if (animator.GetBool("IsMoving") != isMoving)
                animator.SetBool("IsMoving", isMoving);
        }

        #endregion

        #region Networking

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(Activated);
                
                stream.SendNext(startPoint);
                stream.SendNext(targetPoint);
                stream.SendNext(direction);
                stream.SendNext(isMoving);
                stream.SendNext(blockedTime);

                stream.SendNext(prevRoom);
                stream.SendNext(nextRoom);
                stream.SendNext(curFloor);
                stream.SendNext(curNodeNum);

                //stream.SendNext(transform.position);
            }
            else
            {
                Activated = (bool)stream.ReceiveNext();

                startPoint = (Vector2)stream.ReceiveNext();
                targetPoint = (Vector2)stream.ReceiveNext();
                direction = (Vector2)stream.ReceiveNext();
                isMoving = (bool)stream.ReceiveNext();
                blockedTime = (float)stream.ReceiveNext();

                prevRoom = (int)stream.ReceiveNext();
                nextRoom = (int)stream.ReceiveNext();
                curFloor = (int)stream.ReceiveNext();
                curNodeNum = (int)stream.ReceiveNext();

                //transform.position = (Vector3)stream.ReceiveNext();
            }
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            positionVaildCheck();
        }

        /// <summary>
        /// Check this NPC is in the vaild position(it means that this NPC is 'in line'.).
        /// </summary>
        private void positionVaildCheck()
        {
            Vector2 diffVector = targetPoint - startPoint;
            if (diffVector.x == 0f)
                transform.position = new Vector3(startPoint.x, transform.position.y);
            if (diffVector.y == 0f)
                transform.position = new Vector3(transform.position.x, startPoint.y);
        }

        #endregion

        /*
        private void AstarPathfindng(Vector2 start, Vector2 target)
        {
            Vector2 current = start;

            Dictionary<Vector2, float> open = new Dictionary<Vector2, float>();
            List<Vector2> close = new List<Vector2>();

            open.Add(start + Vector2.up, Vector2.Distance(start + Vector2.up, target));
            open.Add(start + Vector2.down, Vector2.Distance(start + Vector2.down, target));
            open.Add(start + Vector2.left, Vector2.Distance(start + Vector2.left, target))
            open.Add(start + Vector2.up, Vector2.Distance(start + Vector2.up, target));
        }

        private bool CheckHit(Vector2 size, Vector2 target)
        {
            RaycastHit2D hit = Physics2D.BoxCast(target, size, 0, new Vector2(), 0f);
            if (hit)
                return true;
            else
                return false;
        }
        */
    }
}