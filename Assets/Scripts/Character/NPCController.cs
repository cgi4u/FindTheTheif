using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// Control a NPC's behavior, especially routing
    /// </summary>
    [RequireComponent(typeof(PhotonTransformView))]
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

        public bool Activated { get; set; } = false;

        MapDataManager mapDataManager;      //Routing Manager Instance에 대한 참조
        // Use this for initialization
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
            
            targetPoint = startPoint = transform.position = (Vector2)curRoute.NodeSet[curNodeNum].transform.position;
        }

        #endregion

        private void Update()
        {
            if (!Activated || (PhotonNetwork.connected && !photonView.isMine))
                return;

            //내가 블록상태든, 충돌방지 대기상태든 내 위치는 가장 최근의 정지 포인트여야 한다.
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
                if ((Vector2)transform.position == curRoute.NodeSet[curNodeNum + 1].position)
                    ChangeNode();
                else if ((Vector2)transform.position == targetPoint)
                    SetNewTargetPoint();
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

        private bool isMoving = false;
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
                Debug.Log("Collision with a player name: " + collision.gameObject.GetComponent<PhotonView>().owner.NickName);
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
        }

        [SerializeField]
        private Vector2 startPoint;
        [SerializeField]
        private Vector2 targetPoint;
        private List<Vector2> directions = new List<Vector2>(new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right } );
        private Vector2 direction = new Vector2(0, 0);
        private void SetNewTargetPoint()
        {
            startPoint = targetPoint;

            // prevent location error can be caused by master client change.
            startPoint.x = Mathf.Round(startPoint.x);
            startPoint.y = Mathf.Round(startPoint.y);

            // Set the prioirty of direction used to set this npc's new tartget point.
            Vector2 nextNodePos = (Vector2)curRoute.NodeSet[curNodeNum + 1].transform.position;
            directions.Sort((x, y)
                => Vector2.Distance(startPoint + x, nextNodePos).CompareTo(Vector2.Distance(startPoint + y, nextNodePos)));

            for (int i = 0; i < directions.Count; i++)
            {
                if (-direction == directions[i])
                {
                    Vector2 temp = directions[directions.Count - 1];
                    directions[directions.Count - 1] = directions[i];
                    directions[i] = temp;
                    break;
                }
            }

            Vector2 newDirection = new Vector2();
            for (int i = 0; i < directions.Count; i++)
            {
                // Check if there is a collider in the travel path of this object.
                RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, directions[i], directions[i].magnitude);

                bool ifHit = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                    {
                        //자신의 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
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
            }
            else
            {
                direction = newDirection;
                targetPoint = startPoint + newDirection;
                isMoving = true;
            }
        }

        [SerializeField]
        private Route curRoute;
        [SerializeField]
        private int curNodeNum;
        /// <summary>
        /// Check if this NPC arrived at next node or route end. If did, change target node or current route. 
        /// </summary>
        private void ChangeNode()
        {
            //GetComponent<PhotonTransformView>().SetSynchronizedValues(new Vector3(0f, 0f), 0f);
            isMoving = false;
            targetPoint = startPoint = curRoute.NodeSet[curNodeNum + 1].position;
            blockedTime = Random.Range(0f, 0.5f);   // Set delay at each node for thief user control easily

            curNodeNum += 1;
            if (curRoute.NodeSet[curNodeNum].IsItemPoint)   // Set item watching time.
                blockedTime = Random.Range(1f, 3f);

            if (curNodeNum == curRoute.NodeSet.Length - 1)       // Route ends, get next route.
            {
                // If this NPC is moving on In-Room Route, next room should be picked in master client and sent to other clients
                int randomNextRoom = -1;
                if (curRoute.RouteType == RouteType.In_Room)
                {
                    randomNextRoom = Random.Range(curRoute.CurRoom + 1,
                                                    curRoute.CurRoom + mapDataManager.Rooms.Count - 1) % mapDataManager.Rooms.Count;
                }

                if (PhotonNetwork.connected)
                {
                    photonView.RPC("ChangeCurRoute", PhotonTargets.All, randomNextRoom);
                }
                else
                    ChangeCurRoute(randomNextRoom);
            }
        }

        private int prevRoom;
        private int nextRoom;
        private int curFloor;
        [PunRPC]
        void ChangeCurRoute(int randomNextRoom)
        {
            switch (curRoute.RouteType)
            {
                case RouteType.In_Room:                 // Currunt root is In Room Route -> Next can be Room to Room/Stair
                    prevRoom = curRoute.CurRoom;
                    nextRoom = randomNextRoom;

                    if (curFloor == mapDataManager.Rooms[nextRoom].Floor)       // The next room is in the same floor -> Room to Room route
                    {
                        //Debug.Log("Case: Room-to-Room");
                        curRoute = null;
                        foreach (Route roomToRoomRoute in mapDataManager.Rooms[prevRoom].ToRoomRoutes)
                        {
                            if (roomToRoomRoute.EndRoom == nextRoom)
                            {
                                curRoute = roomToRoomRoute;
                                break;
                            }
                        }

                        if (curRoute == null)
                        {
                            Debug.LogError("Room " + prevRoom + " to Room " + nextRoom + " Route is not set.");
                            return;
                        }
                    }
                    else        // The next room is in one of the other floors -> Room to Stair route
                    {
                        string sideAndType;

                        //Debug.Log("Case: Room-to-Stair");
                        if (mapDataManager.Rooms[nextRoom].Floor < curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.Left)
                        {
                            sideAndType = "Left Down";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.LeftDownRoute;
                        }
                        else if (mapDataManager.Rooms[nextRoom].Floor > curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.Left)
                        {
                            sideAndType = "Left Up";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.LeftUpRoute;
                        }
                        else if (mapDataManager.Rooms[nextRoom].Floor < curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.Right)
                        {
                            sideAndType = "Right Down";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.RightDownRoute;
                        }
                        else
                        {
                            sideAndType = "Right Up";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.RightUpRoute;
                        }

                        if (curRoute == null)
                        {
                            Debug.LogError("Room " + prevRoom + " to " + sideAndType + " Stair Route is not set.");
                            return;
                        }
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
                        //Debug.Log("Case: Stair-to-Room");
                        string sideAndType;

                        if (curRoute.StairSide == StairSide.Left && curRoute.StairType == StairType.Down)   // From Stair Route starts in the side where previous route ends
                        {
                            sideAndType = "Left Up";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.LeftUpRoute;
                        }
                        else if (curRoute.StairSide == StairSide.Left && curRoute.StairType == StairType.Up)
                        {
                            sideAndType = "Left Down";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.LeftDownRoute;
                        }
                        else if (curRoute.StairSide == StairSide.Right && curRoute.StairType == StairType.Down)
                        {
                            sideAndType = "Right Up";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.RightUpRoute;
                        }
                        else
                        {
                            sideAndType = "Right Down";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.RightDownRoute;
                        }

                        if (curRoute == null)
                        {
                            Debug.LogError(sideAndType + " Stair to Room " + nextRoom + " Route is not set.");
                            return;
                        }
                    }
                    else   // The target room isn't in this floor -> Stair to Stair Route
                    {
                        string sideAndType;

                        if (curRoute.StairSide == StairSide.Left && curRoute.StairType == StairType.Down)   // From Stair Route starts in the side where previous route ends
                        {
                            sideAndType = "Left Down Down";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].LeftDownRoute;
                        }
                        else if (curRoute.StairSide == StairSide.Left && curRoute.StairType == StairType.Up)
                        {
                            sideAndType = "Left Up Up";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].LeftUpRoute;
                        }
                        else if (curRoute.StairSide == StairSide.Right && curRoute.StairType == StairType.Down)
                        {
                            sideAndType = "Right Down Down";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].RightDownRoute;
                        }
                        else
                        {
                            sideAndType = "Right Up Up";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].RightUpRoute;
                        }

                        if (curRoute == null)
                        {
                            Debug.LogError(sideAndType + " Stair to Stair Route in " + curFloor + "th Floor is not set.");
                            return;
                        }
                    }
                    break;
                default:    // The current route is To Room Route -> Next is In Room Route
                    //Debug.Log("Case: In-Room");
                    curRoute = mapDataManager.Rooms[nextRoom].InRoomRoute;

                    if (curRoute == null)
                    {
                        Debug.LogError("In Room Route of " + nextRoom + "th room is not set.");
                        return;
                    }
                    break;
            }
            curNodeNum = 0;

            if (!PhotonNetwork.connected || photonView.isMine)
            {
                transform.position = curRoute.NodeSet[curNodeNum].transform.position;
                //SetNewTargetPoint();
            }
               
        }

        #endregion

        #region Networking

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(startPoint);
                stream.SendNext(targetPoint);
                stream.SendNext(isMoving);
                stream.SendNext(blockedTime);

                stream.SendNext(prevRoom);
                stream.SendNext(nextRoom);
                stream.SendNext(curNodeNum);

                //stream.SendNext(transform.position);
            }
            else
            {
                startPoint = (Vector2)stream.ReceiveNext();
                targetPoint = (Vector2)stream.ReceiveNext();
                isMoving = (bool)stream.ReceiveNext();
                blockedTime = (float)stream.ReceiveNext();

                prevRoom = (int)stream.ReceiveNext();
                nextRoom = (int)stream.ReceiveNext();
                curNodeNum = (int)stream.ReceiveNext();

                //transform.position = (Vector3)stream.ReceiveNext();
            }
        }

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (curNodeNum == curRoute.NodeSet.Length - 1)
            {
                positionVaildCheck(curRoute.NodeSet[curNodeNum].transform.position,
                                    curRoute.NodeSet[curNodeNum - 1].transform.position);
            }
            else
            {
                positionVaildCheck(curRoute.NodeSet[curNodeNum + 1].transform.position,
                                    curRoute.NodeSet[curNodeNum].transform.position);
            }
        }

        /// <summary>
        /// Check this NPC is in the vaild position(it means that this NPC is 'in line'.).
        /// </summary>
        private void positionVaildCheck(Vector2 destVector, Vector2 startVector)
        {
            Vector2 diffVector = destVector - startVector;
            if (diffVector.x == 0f)
                transform.position = new Vector3(startVector.x, transform.position.y);
            if (diffVector.y == 0f)
                transform.position = new Vector3(transform.position.x, startVector.y);
        }

        #endregion
    }
}