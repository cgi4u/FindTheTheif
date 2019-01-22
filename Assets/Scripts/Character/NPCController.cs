using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // NPC의 행동 제어 (경로 순환)
    public class NPCController : Photon.PunBehaviour, IPunObservable
    {
        [SerializeField]
        public Route curRoute;              // 현재 진행중인 경로
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
        public void ManualStart(RouteNode startPoint)
        {
            mapDataManager = MapDataManager.Instance;

            curRoute = startPoint.gameObject.GetComponentInParent<Route>();
            routeNodeSet = curRoute.NodeSet;

            switch (curRoute.routeType)
            {
                case Route.RouteType.In_Room:
                    nextRoom = curRoute.curRoom;
                    break;
                case Route.RouteType.Room_to_Room:
                case Route.RouteType.Stair_to_Room:
                    nextRoom = curRoute.endRoom;
                    break;
                default:
                    Debug.LogError("Route type error.");
                    break;
            }

            curFloor = mapDataManager.RoomFloor[nextRoom];
            for (int i = 0; i < curRoute.NodeSet.Length; i++)
            {
                if (startPoint == curRoute.NodeSet[i])
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

        private void RouteCheck()
        {
            if (transform.position.Equals(routeNodeSet[curNodeNum + 1].transform.position))
            {
                //transform.position = routeNodeSet[curNodeNum + 1].transform.position;

                curNodeNum += 1;
                if (curNodeNum < routeNodeSet.Length - 1)
                {
                    if (routeNodeSet[curNodeNum].ifItemPoint)
                    {
                        blockedTime = Random.Range(1, 3);
                        isMoving = false;
                    }
                }
                else
                {
                    switch (curRoute.routeType)
                    {
                        case Route.RouteType.In_Room:
                            prevRoom = nextRoom;
                            do
                            {
                                nextRoom = Random.Range(0, 5);
                            } while (prevRoom == nextRoom);

                            //다음 방의 층수에 따라 다음 경로의 형태가 정해진다]
                            if (curFloor == mapDataManager.RoomFloor[nextRoom])      // 같은 층 내에서에 이동
                            {
                                //Debug.Log("Case: Room-to-Room");
                                curRoute = mapDataManager.RoomToRoomRoutes[prevRoom][nextRoom];
                            }
                            else if (curFloor > mapDataManager.RoomFloor[nextRoom])  // 내려가는 계단으로 이동
                            {
                                //Debug.Log("Case: Room-to-down");
                                curRoute = mapDataManager.RoomToStairRoutes[prevRoom][0];
                            }
                            else                                                                    // 올라가는 계단으로 이동
                            {
                                //Debug.Log("Case: Room-to-up");
                                curRoute = mapDataManager.RoomToStairRoutes[prevRoom][1];
                            }
                            
                            break;
                        case Route.RouteType.Room_to_Stair:
                        case Route.RouteType.Stair_to_Stair:
                            //StopCoroutine("MoveCheck");
                            if (curRoute.stairType == Route.StairType.down) // 계단을 통해 내려옴 -> 올라가는 계단에서 해당 방으로
                            {
                                curFloor -= 1;

                                if (mapDataManager.RoomFloor[nextRoom] == curFloor)
                                {
                                    //Debug.Log("Case: Stair-to-Room");
                                    curRoute = mapDataManager.StairToRoomRoutes[nextRoom][1];
                                }
                                else
                                {
                                    if (curRoute.stairSide == Route.StairSide.left)
                                    {
                                        //Debug.Log("Case: Down-down left");
                                        curRoute = mapDataManager.StairToStairRoutes[curFloor][0];
                                    }
                                    else
                                    {
                                        //Debug.Log("Case: Down-down right");
                                        curRoute = mapDataManager.StairToStairRoutes[curFloor][2];
                                    }
                                }

                            }
                            else                                            // 계단을 통해 올라옴 -> 내려가는 계단에서 해당 방으로
                            {
                                curFloor += 1;

                                if (mapDataManager.RoomFloor[nextRoom] == curFloor)
                                {
                                    //Debug.Log("Case: Stair-to-Room");
                                    curRoute = mapDataManager.StairToRoomRoutes[nextRoom][0];
                                }
                                else
                                {
                                    if (curRoute.stairSide == Route.StairSide.left)
                                    {
                                        //Debug.Log("Case: Up-up left");
                                        curRoute = mapDataManager.StairToStairRoutes[curFloor][1];
                                    }
                                    else
                                    {
                                        //Debug.Log("Case: Up-up left");
                                        curRoute = mapDataManager.StairToStairRoutes[curFloor][3];
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