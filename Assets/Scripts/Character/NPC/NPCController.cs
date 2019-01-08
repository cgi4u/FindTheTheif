using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // NPC의 행동 제어 (경로 순환)
    public class NPCController : CharController
    {
        public Route curRoute;                     //현재 진행중인 경로
        private RouteNode[] routeNodeSet;   // 현재 진행중인 경로의 노드집합
        [SerializeField]
        private int curNodeNum;             // 가장 마지막으로 지난 노드의 인덱스
        private int curFloor;               //현재 층

        RoutingManager routingManager;      //Routing Manager Instance에 대한 참조

        public int[] routingSequence;       //NPC가 따르는 이동 시퀀스
                                            //지나는 방 번호가 순서대로 기록됨
        int seqIdx;                         //시퀀스 내에서 현재 목표로 하는, 또는 경유중인 인덱스

        private new void Awake()
        {
            base.Awake();   // Raycast box initiallize

            //routingManager = RoutingManager.Instance;
        }

        // Use this for initialization
        void Start()
        {
            if (!PhotonNetwork.connected)
                ManualStart(curRoute);

            blockedTime = 1;

            /*
            routeNodeSet = curRoute.NodeSet;
            curNodeNum = 0;
            targetRoom = curRoute.curRoom;
            curFloor = routingManager.RoomFloor[targetRoom];
            transform.position = (Vector2)routeNodeSet[curNodeNum].transform.position;

            StartCoroutine("MoveCheck");*/
        }

        public void ManualStart(Route startRoute)
        {
            routingManager = RoutingManager.Instance;

            curRoute = startRoute;
            routeNodeSet = curRoute.NodeSet;
            
            switch (curRoute.routeType)
            {
                case Route.RouteType.In_Room:
                    targetRoom = curRoute.curRoom;
                    break;
                case Route.RouteType.Room_to_Room:
                case Route.RouteType.Stair_to_Room:
                    targetRoom = curRoute.endRoom;
                    break;
                default:
                    Debug.LogError("Route type error.");
                    break;
            }

            curFloor = routingManager.RoomFloor[targetRoom];

            
            bool ifOccupied;
            bool[] nodeOccupied = new bool[routeNodeSet.Length - 1];
            bool allOcuppiedFlag = true;

            /*if (!PhotonNetwork.connected)   //For offline manual test
            {
                curNodeNum = 0;
                transform.position = (Vector2)routeNodeSet[curNodeNum].transform.position;
                StartCoroutine("MoveCheck");
                blockedTime = 0;

                return;
            }*/

            int randNodeIdx = Random.Range(1, routeNodeSet.Length - 1); ;
            /*RaycastHit2D[] hits = Physics2D.BoxCastAll(routeNodeSet[randNodeIdx].transform.position, raycastBox, 0, new Vector2(0, 0), 0.0f);
            ifOccupied = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (!hit.collider.isTrigger)
                {
                    ifOccupied = true;
                    nodeOccupied[randNodeIdx] = true;
                }
            }

            while (ifOccupied)
            {
                for (int i = 0; i < nodeOccupied.Length; i++)
                {
                    if (nodeOccupied[i] == false)
                    {
                        allOcuppiedFlag = false;
                        break;
                    }
                }

                if (allOcuppiedFlag == true)
                    break;

                randNodeIdx = Random.Range(1, routeNodeSet.Length - 1);
                hits = Physics2D.BoxCastAll(routeNodeSet[randNodeIdx].transform.position, raycastBox, 0, new Vector2(0, 0), 0.0f);
                ifOccupied = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (!hit.collider.isTrigger)
                    {
                        ifOccupied = true;
                        nodeOccupied[randNodeIdx] = true;
                    }
                }
            }

            if (allOcuppiedFlag == true)
            {
                Debug.LogError("All Route Node is occupied: Error");
                Debug.LogError("Route: " + curRoute.gameObject.name);
                gameObject.SetActive(false);
                return;
            }*/

            curNodeNum = randNodeIdx;
            transform.position = (Vector2)routeNodeSet[curNodeNum].transform.position;

            StartCoroutine("MoveCheck");
            blockedTime = 0;
        }

        [SerializeField]
        bool blocked = false;
        int blockedTime = 0;
        int itemWatchingTime = 0;
        // Update is called once per frame
        void Update()
        {
            //Debug.Log(targetPoint);
            if (blockedTime == 0 && curNodeNum < routeNodeSet.Length - 1)
            {
                Move();
                RouteEndCheck();
            }
        }

        protected new void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);

            blocked = true;
            blockedTime = 1;
            if (collision.gameObject.tag == "NPC"
                && collision.gameObject.GetInstanceID() > gameObject.GetInstanceID())
            {
                    blockedTime += 1;
            }
                
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

                if (blocked)
                    blocked = !blocked;

                if (blockedTime > 0)
                {
                    blockedTime -= 1;
                    if (blockedTime != 0)
                    {
                        yield return new WaitForSeconds(1.0f / moveSpeed);
                        continue;
                    }
                }

                /*if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].position) < 0.01f
                    && curNodeNum < routeNodeSet.Length)
                {
                    transform.position = routeNodeSet[curNodeNum + 1].position;
                    curNodeNum++;
                }*/

                startPoint = (Vector2)transform.position;   // Set starting point
                if (curNodeNum < routeNodeSet.Length - 1)
                    direction = ((Vector2)routeNodeSet[curNodeNum + 1].transform.position - startPoint).normalized;
                else
                    direction = new Vector2(0, 0);
                targetPoint = startPoint + direction;

                //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
                //자기자신의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
                RaycastHit2D[] hits = Physics2D.BoxCastAll(targetPoint, raycastBox, 0, new Vector2(0, 0), 0.0f);

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
                {
                    blockedTime += 1;
                    blocked = true;
                }

                yield return new WaitForSeconds(1.0f / moveSpeed);
            }
        }

        int prevRoom;
        int targetRoom;

        private void RouteEndCheck()
        {
            if (Vector2.Distance(transform.position, routeNodeSet[curNodeNum + 1].transform.position) < 0.05f)
            {
                transform.position = routeNodeSet[curNodeNum + 1].transform.position;

                curNodeNum += 1;
                if (curNodeNum < routeNodeSet.Length - 1)
                {
                    if (routeNodeSet[curNodeNum].ifItemPoint)
                    {
                        itemWatchingTime = (int)Random.Range(moveSpeed, moveSpeed * 4 - 1);
                    }
                }
                else
                {
                    Debug.Log("Route change");

                    switch (curRoute.routeType)
                    {
                        case Route.RouteType.In_Room:
                            

                            prevRoom = targetRoom;
                            do
                            {
                                targetRoom = Random.Range(0, 5);
                            } while (prevRoom == targetRoom);

                            //다음 방의 층수에 따라 다음 경로의 형태가 정해진다]
                            if (curFloor == routingManager.RoomFloor[targetRoom])      // 같은 층 내에서에 이동
                            {
                                Debug.Log("Case: Room-to-Room");
                                curRoute = routingManager.RoomToRoomRouteSet[prevRoom][targetRoom];
                            }
                            else if (curFloor > routingManager.RoomFloor[targetRoom])  // 내려가는 계단으로 이동
                            {
                                Debug.Log("Case: Room-to-down");
                                curRoute = routingManager.RoomToStairRouteSet[prevRoom][0];
                            }
                            else                                                                    // 올라가는 계단으로 이동
                            {
                                Debug.Log("Case: Room-to-up");
                                curRoute = routingManager.RoomToStairRouteSet[prevRoom][1];
                            }
                            
                            break;
                        case Route.RouteType.Room_to_Stair:
                        case Route.RouteType.Stair_to_Stair:
                            Route nextCurRoute;
                            StopCoroutine("MoveCheck");
                            if (curRoute.stairType == Route.StairType.down) // 계단을 통해 내려옴 -> 올라가는 계단에서 해당 방으로
                            {
                                curFloor -= 1;

                                if (routingManager.RoomFloor[targetRoom] == curFloor)
                                {
                                    Debug.Log("Case: Stair-to-Room");
                                    nextCurRoute = routingManager.StairToRoomRouteSet[targetRoom][1];
                                }
                                else
                                {
                                    if (curRoute.stairSide == Route.StairSide.left)
                                    {
                                        Debug.Log("Case: Down-down left");
                                        nextCurRoute = routingManager.StairToStairRouteSet[curFloor][0];
                                    }
                                    else
                                    {
                                        Debug.Log("Case: Down-down right");
                                        nextCurRoute = routingManager.StairToStairRouteSet[curFloor][2];
                                    }
                                }

                            }
                            else                                            // 계단을 통해 올라옴 -> 내려가는 계단에서 해당 방으로
                            {
                                curFloor += 1;

                                if (routingManager.RoomFloor[targetRoom] == curFloor)
                                {
                                    Debug.Log("Case: Stair-to-Room");
                                    nextCurRoute = routingManager.StairToRoomRouteSet[targetRoom][0];
                                }
                                else
                                {
                                    if (curRoute.stairSide == Route.StairSide.left)
                                    {
                                        Debug.Log("Case: Up-up left");
                                        nextCurRoute = routingManager.StairToStairRouteSet[curFloor][1];
                                    }
                                    else
                                    {
                                        Debug.Log("Case: Up-up left");
                                        nextCurRoute = routingManager.StairToStairRouteSet[curFloor][3];
                                    }
                                }
                            }
                            transform.position = (Vector2)nextCurRoute.NodeSet[0].transform.position;
                            StartCoroutine("MoveCheck");

                            curRoute = nextCurRoute;
                            break;
                        default: 
                            Debug.Log("Case: In-Room");
                            curRoute = routingManager.InRoomRouteSet[curRoute.endRoom];
                            break;
                    }

                    routeNodeSet = curRoute.NodeSet;
                    curNodeNum = 0;
                }
            }
        }

        #endregion

    }
}
