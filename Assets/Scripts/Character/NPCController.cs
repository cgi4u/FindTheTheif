using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // NPC의 행동 제어 (경로 순환)
    public class NPCController : Photon.PunBehaviour, IPunObservable
    {
        [SerializeField]
        private Route curRoute;              // 현재 진행중인 경로

        #region Current Route Networking

        [SerializeField]
        private int curRouteRoom;
        [SerializeField]
        private RouteType curRouteType;
        [SerializeField]
        private StairSide curRouteStairSide;
        [SerializeField]
        private StairType curRouteStairType;

        #endregion

        [SerializeField]
        private RouteNode[] routeNodeSet;   // 현재 진행중인 경로의 노드집합
        [SerializeField]
        private int curNodeNum;             // 가장 마지막으로 지난 노드의 인덱스
        [SerializeField]
        private int curFloor;               //현재 층
        [SerializeField]
        private bool ifStarted = false;     //ManualStart를 통해 초기화되었는지에 대한 변수
        [SerializeField]
        MapDataManager mapDataManager;      //Routing Manager Instance에 대한 참조

        private Vector2 raycastBox; // Collider of a characters

        private void Awake()
        {
            raycastBox = GetComponent<BoxCollider2D>().size;   // To ignore collisions on edges
        }

        // Use this for initialization
        void Start()
        {
            //오프라인 테스트시 수동으로 생성하는 인스턴스에 대한 초기화
            if (mapDataManager == null)
                mapDataManager = MapDataManager.Instance;
        }

        [PunRPC]
        public void ManualStart(int genPointIdx)
        {
            mapDataManager = MapDataManager.Instance;

            RouteNode genPoint = mapDataManager.NPCGenPoints[genPointIdx];
            curRoute = genPoint.gameObject.GetComponentInParent<Route>();
            routeNodeSet = curRoute.NodeSet;

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
            transform.position = (Vector2)routeNodeSet[curNodeNum].transform.position;
            ifStarted = true;
        }

        void Update()
        {
            if (!ifStarted || (PhotonNetwork.connected && !photonView.isMine))
                return;

            if (blockedTime > 0f)
            {
                blockedTime -= Time.deltaTime;
                if (blockedTime <= 0f)
                {
                    blockedTime = 0f;
                }
                else return;
            }

            if (isMoving)
                Move();
            else
                SetNewTargetPoint();
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (PhotonNetwork.connected && !photonView.isMine)
                return;

            if (collision.gameObject.tag != "NPC"
                || collision.gameObject.GetInstanceID() > gameObject.GetInstanceID())
            {
                transform.position = startPoint;
                isMoving = false;
            }
        }

        #region NPC Routing

        [SerializeField]
        private Vector2 startPoint;
        [SerializeField]
        private Vector2 targetPoint;
        public float moveSpeed;

        [SerializeField]
        bool isMoving = false;
        [SerializeField]
        float blockedTime = 0;

        private void Move()
        {
            //설정 속도에 따라 움직일 위치를 계산(MoveTowards) 이후 이동
            Vector2 nextPos = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            transform.position = (Vector3)nextPos;

            if (transform.position.Equals(targetPoint))
            {
                RouteCheck();
                SetNewTargetPoint();
            }

            //이동 위치에 따라 스프라이트 우선순위를 결정(y축 위치가 더 큰 캐릭터가 뒤로 가도록)
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);

            return;
        }

        Vector2 direction = new Vector2(0, 0);
        private void SetNewTargetPoint()
        {
            startPoint = (Vector2)transform.position;   // 현재 위치를 새로운 startPoint로 설정
            if (curNodeNum < routeNodeSet.Length - 1)
                direction = ((Vector2)routeNodeSet[curNodeNum + 1].transform.position - startPoint).normalized;
            else
                direction = new Vector2(0, 0);
            targetPoint = startPoint + direction;

            //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
            //자기자신의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
            RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, targetPoint - startPoint, Vector2.Distance(startPoint, targetPoint));

            //자기 자신 이외에 충돌 물체가 있다면 이동하지 않는다.
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

            if (ifHit)
                isMoving = false;
            else
                isMoving = true;
        }

        [SerializeField]
        int prevRoom;
        [SerializeField]
        int nextRoom;

        /// <summary>
        /// Check if this NPC arrived at next node or route end. If did, change target node or current route. 
        /// </summary>
        private void RouteCheck()
        {
            if (transform.position.Equals(routeNodeSet[curNodeNum + 1].transform.position))
            {
                curNodeNum += 1;
                if (curNodeNum < routeNodeSet.Length - 1)       // Route not ends, arrived at next node.
                {
                    if (routeNodeSet[curNodeNum].IfItemPoint)   // Set item watching time.
                    {
                        blockedTime = Random.Range(1, 3);
                        isMoving = false;
                    }
                }
                else                                            // Route ends, get next route.
                {
                    switch (curRoute.RouteType)
                    { 
                        case RouteType.In_Room:                 // Currunt root is In Room Route -> Next can be Room to Room/Stair
                            prevRoom = nextRoom;
                            do
                            {
                                nextRoom = Random.Range(0, mapDataManager.Rooms.Length);
                            } while (prevRoom == nextRoom);

                            if (curFloor == mapDataManager.Rooms[nextRoom].Floor)       // The next room is in the same floor -> Room to Room route
                            {
                                //Debug.Log("Case: Room-to-Room");
                                curRouteType = RouteType.Room_to_Room;
                            }
                            else                                                         // The next room is in one of the other floors -> Room to Stair route
                            {
                                //Debug.Log("Case: Room-to-up");
                                curRouteType = RouteType.Room_to_Stair;
                                curRouteStairSide = mapDataManager.Rooms[prevRoom].AdjStairSide;
                                if (curFloor > mapDataManager.Rooms[nextRoom].Floor)
                                    curRouteStairType = StairType.down;
                                else
                                    curRouteStairType = StairType.up;
                            }
                            
                            break;
                        case RouteType.Room_to_Stair:              // Current route is To Stair Route -> Next can be Stair to Room/Stair
                        case RouteType.Stair_to_Stair:
                            if (curRoute.StairType == StairType.down)       // Current route ends with down stair -> Next starts with up stair
                            {
                                curFloor -= 1;

                                if (mapDataManager.Rooms[nextRoom].Floor == curFloor)   // The target room is in this floor.
                                {
                                    //Debug.Log("Case: Stair-to-Room");
                                    curRouteRoom = nextRoom;
                                    curRouteStairSide = curRoute.StairSide;             // From Stair Route starts in the side where previous route ends
                                    curRouteType = 
                                }
                                else
                                {
                                    if (curRoute.StairSide == Route.StairSide.left)
                                    {
                                        //Debug.Log("Case: Down-down left");
                                        curRoute = mapDataManager.StairToStairRoutes[(curFloor - 1) * mapDataManager.maxFloorNum];
                                    }
                                    else
                                    {
                                        //Debug.Log("Case: Down-down right");
                                        curRoute = mapDataManager.StairToStairRoutes[(curFloor - 1) * mapDataManager.maxFloorNum + 2];
                                    }
                                }

                            }
                            else                                            // 계단을 통해 올라옴 -> 내려가는 계단에서 해당 방으로
                            {
                                curFloor += 1;

                                if (mapDataManager.RoomFloor[nextRoom] == curFloor)
                                {
                                    //Debug.Log("Case: Stair-to-Room");
                                    curRoute = mapDataManager.StairToRoomRoutes[nextRoom * 2];
                                }
                                else
                                {
                                    if (curRoute.StairSide == Route.StairSide.left)
                                    {
                                        //Debug.Log("Case: Up-up left");
                                        curRoute = mapDataManager.StairToStairRoutes[(curFloor - 1) * mapDataManager.maxFloorNum + 1];
                                    }
                                    else
                                    {
                                        //Debug.Log("Case: Up-up left");
                                        curRoute = mapDataManager.StairToStairRoutes[(curFloor - 1) * mapDataManager.maxFloorNum + 3];
                                    }
                                }
                            }
                            transform.position = (Vector2)curRoute.NodeSet[0].transform.position;
                            //StartCoroutine("MoveCheck");
                            break;
                        default: 
                            //Debug.Log("Case: In-Room");
                            curRoute = mapDataManager.InRoomRoutes[curRoute.endRoom];
                            break;
                    }

                    routeNodeSet = curRoute.NodeSet;
                    curNodeNum = 0;
                }
            }
        }

        [PunRPC]
        void RenewCurRoute(int roomNum, RouteType routeType, int targetRoom, StairType stairType, StairSide stairSide, int floor)
        {

        }

        #endregion

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

                //stream.SendNext(curRoute);
            }
            else
            {
                startPoint = (Vector2)stream.ReceiveNext();
                targetPoint = (Vector2)stream.ReceiveNext();
                isMoving = (bool)stream.ReceiveNext();
                blockedTime = (float)stream.ReceiveNext();
                prevRoom = (int)stream.ReceiveNext();
                nextRoom = (int)stream.ReceiveNext();

                //curRoute = (Route)stream.ReceiveNext();
            }
        }
    }
}