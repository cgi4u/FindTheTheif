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

        /*
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
        */

        [SerializeField]
        private RouteNode[] routeNodeSet;   // 현재 진행중인 경로의 노드집합
        [SerializeField]
        private int curNodeNum;             // 가장 마지막으로 지난 노드의 인덱스
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
        public void Init(int genPointIdx)
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
        /// The number of floor in which this NPC exists. Index 0 is used for the 1st floor
        /// </summary>
        [SerializeField]
        private int curFloor;

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
                    if (PhotonNetwork.connected)
                        photonView.RPC("RenewCurRoute", PhotonTargets.All);
                    else
                        RenewCurRoute();
                }
            }
        }

        [PunRPC]
        void RenewCurRoute()
        {
            switch (curRoute.RouteType)
            {
                case RouteType.In_Room:                 // Currunt root is In Room Route -> Next can be Room to Room/Stair
                    prevRoom = nextRoom;
                    do
                    {
                        nextRoom = Random.Range(0, mapDataManager.Rooms.Count);
                    } while (prevRoom == nextRoom);

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
                        if (mapDataManager.Rooms[nextRoom].Floor < curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.left)
                        {
                            sideAndType = "Left Down";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.LeftDownRoute;
                        }
                        else if (mapDataManager.Rooms[nextRoom].Floor > curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.left)
                        {
                            sideAndType = "Left Up";
                            curRoute = mapDataManager.Rooms[prevRoom].ToStairRoutes.LeftUpRoute;
                        }
                        else if (mapDataManager.Rooms[nextRoom].Floor < curFloor && mapDataManager.Rooms[prevRoom].AdjStairSide == StairSide.right)
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
                    if (curRoute.StairType == StairType.down)       // Current route ends with down stair
                        curFloor -= 1;
                    else
                        curFloor += 1;

                    if (mapDataManager.Rooms[nextRoom].Floor == curFloor)   // The target room is in this floor -> Stair to Room Route
                    {
                        //Debug.Log("Case: Stair-to-Room");
                        string sideAndType;

                        if (curRoute.StairSide == StairSide.left && curRoute.StairType == StairType.down)   // From Stair Route starts in the side where previous route ends
                        {
                            sideAndType = "Left Up";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.LeftUpRoute;
                        }
                        else if (curRoute.StairSide == StairSide.left && curRoute.StairType == StairType.up)
                        {
                            sideAndType = "Left Down";
                            curRoute = mapDataManager.Rooms[nextRoom].FromStairRoutes.LeftDownRoute;
                        }
                        else if (curRoute.StairSide == StairSide.right && curRoute.StairType == StairType.down)
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

                        if (curRoute.StairSide == StairSide.left && curRoute.StairType == StairType.down)   // From Stair Route starts in the side where previous route ends
                        {
                            sideAndType = "Left Down Down";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].LeftDownRoute;
                        }
                        else if (curRoute.StairSide == StairSide.left && curRoute.StairType == StairType.up)
                        {
                            sideAndType = "Left Up Up";
                            curRoute = mapDataManager.StairToStairRoutes[curFloor - 1].LeftUpRoute;
                        }
                        else if (curRoute.StairSide == StairSide.right && curRoute.StairType == StairType.down)
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

            routeNodeSet = curRoute.NodeSet;
            curNodeNum = 0;

            if (!PhotonNetwork.connected || photonView.isMine)
                transform.position = routeNodeSet[curNodeNum].transform.position;
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
                stream.SendNext(curNodeNum);

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
                curNodeNum = (int)stream.ReceiveNext();

                //curRoute = (Route)stream.ReceiveNext();
            }
        }
    }
}