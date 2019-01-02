using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;


namespace com.MJT.FindTheTheif
{
    public class PlayerController : CharController, IPunObservable
    {
        #region Private Properties

        public enum Direction { stop, up, down, right, left };

        private static PlayerController localPlayer; // Singleton of the local player
                                                     // 생각: GameObject로 해야 하나? 버튼 말고 다른데서 얘한테 접근할일이 있나?    
        public static PlayerController LocalPlayer
        {
            get
            {
                return localPlayer;
            }
        }

        private int[] buttons = new int[4];         // The state of each button(Pushed or not pushed, pushed order)
                                                    // Up = 0, Down = 1, Left = 2, Right = 3
        private int btnCount = 0;                   // The number of buttons pushed now.

        private bool ifCheckMove = false;                   // Flag for MoveCheck. Used to stop coroutine.
        private bool isCheckRunning = false;                // Flag to prevent to make a new coroutine.

        public Team TeamOfPlayer { get; set; }

        #endregion

        #region Public Properties

        //public GameObject myCollider;

        #endregion

        #region Unity Callbacks

        private new void Awake()
        {
            base.Awake();   // Raycast box initiallize

            moveSpeed = 5.0f;

            raycastBox = GetComponent<BoxCollider2D>().size;

            if (!PhotonNetwork.connected || photonView.isMine)
            {
                if (localPlayer == null)
                {
                    localPlayer = this;
                    Debug.Log("Local Player Set");
                }
                else
                    Debug.Log("Error: Multiple instantiation of the local player.");
            }
        }

        // Use this for initialization
        void Start()
        {
            CameraWork _cameraWork = this.gameObject.GetComponent<CameraWork>();

            if (_cameraWork != null)
            {
                if (photonView.isMine)
                {
                    _cameraWork.OnStartFollowing();
                }
            }
            else
            {
                Debug.LogError("<Color=Red><a>Missing</a></Color> CameraWork Component on playerPrefab.", this);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (isCheckRunning)
                Move();
        }

        protected new void OnCollisionEnter2D(Collision2D collision)
        {
            base.OnCollisionEnter2D(collision);

            if (isCheckRunning)
            {
                isCheckRunning = false;
                StopCoroutine("MoveCheck");
            }
        }

        #endregion


        #region Public Methods

        public void SetTeam(Team team)
        {
            TeamOfPlayer = team;
        }

        public void OnMoveButtonPushed(string dir)
        {
            //현재 눌린 버튼의 갯수를 저장
            btnCount += 1;

            //현재 눌린 버튼에 해당하는 배열 번호에 갯수와 동일한 수를 저장
            //어떤 버튼이 마지막으로 눌렸는지를 판단하여 이동한다.
            switch (dir)
            {
                case "up":
                    buttons[0] = btnCount;
                    break;
                case "down":
                    buttons[1] = btnCount;
                    break;
                case "left":
                    buttons[2] = btnCount;
                    break;
                case "right":
                    buttons[3] = btnCount;
                    break;
                default:
                    Debug.Log("Error: Undefined input for moving.");
                    break;
            }

            if (btnCount == 1)
            {
                //Debug.Log("First pushed");
                ifCheckMove = true;
                if (!isCheckRunning)
                {
                    StartCoroutine("MoveCheck");
                    //StartCoroutine("MoveCheck2");
                }
            }
        }

        public void OnMoveButtonReleased(string dir)
        {
            switch (dir)
            {
                case "up":
                    buttons[0] = 0;
                    break;
                case "down":
                    buttons[1] = 0;
                    break;
                case "left":
                    buttons[2] = 0;
                    break;
                case "right":
                    buttons[3] = 0;
                    break;
                default:
                    Debug.Log("Error: Undefined input for moving.");
                    break;
            }

            for (int i = 0; i < 4; i++)
            {
                if (buttons[i] > btnCount)
                    buttons[i] -= 1;
            }
            btnCount -= 1;

            if (btnCount == 0)
            {
                //Debug.Log("Last released");
                ifCheckMove = false;
            }
        }

        #endregion


        #region Private Methods

        protected override IEnumerator MoveCheck()
        {
            //isCheckRunning = true;

            while (ifCheckMove)
            {
                isCheckRunning = true;

                startPoint = (Vector2)transform.position + spriteOffset;   // Set starting point

                // Set target point
                if (btnCount == buttons[0])
                {
                    targetPoint = startPoint + Vector2.up;
                }
                else if (btnCount == buttons[1])
                {
                    targetPoint = startPoint + Vector2.down;
                }
                else if (btnCount == buttons[2])
                {
                    targetPoint = startPoint + Vector2.left;
                }
                else if (btnCount == buttons[3])
                {
                    targetPoint = startPoint + Vector2.right;
                }

                //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
                //플레이어(자기자신)의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
                RaycastHit2D[] hits = Physics2D.BoxCastAll(targetPoint, raycastBox, 0, new Vector2(0, 0), 0.0f);

                //플레이어 자기 자신 이외에 충돌 물체가 있다면 이동하지 않는다.
                bool ifHit = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                    {
                        //플레이어 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
                        //Debug.Log(hit.collider.gameObject.name);
                        ifHit = true;
                        break;
                    }
                }

                if (ifHit)
                    break;
                targetPoint = targetPoint - spriteOffset;
                //else
                //    myCollider.transform.position = targetPoint;

                yield return new WaitForSeconds(1.0f / moveSpeed);
            }

            isCheckRunning = false;
        }

        /*protected IEnumerator MoveCheck2()
        {
            isCheckRunning = true;

            while (ifCheckMove)
            {
                Debug.Log("Check Start");
                startPoint = (Vector2)transform.position;   // Set starting point

                // Set target point
                if (btnCount == buttons[0])
                {
                    targetPoint = startPoint + Vector2.up;
                }
                else if (btnCount == buttons[1])
                {
                    targetPoint = startPoint + Vector2.down;
                }
                else if (btnCount == buttons[2])
                {
                    targetPoint = startPoint + Vector2.left;
                }
                else if (btnCount == buttons[3])
                {
                    targetPoint = startPoint + Vector2.right;
                }

                //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
                //플레이어(자기자신)의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
                RaycastHit2D[] hits = Physics2D.BoxCastAll(targetPoint, raycastBox, 0, new Vector2(0, 0), 0.0f);

                //플레이어 자기 자신 이외에 충돌 물체가 있다면 이동하지 않는다.
                bool ifHit = false;
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                    {
                        //플레이어 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
                        //Debug.Log(hit.collider.gameObject.name);
                        ifHit = true;
                        break;
                    }
                }

                if (ifHit) break;
                else Move2();

                yield return new WaitForSeconds(1.0f / moveSpeed);
            }

            isCheckRunning = false;
        }*/

        #endregion


        #region Photon Methods

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            //if (stream.isReading && teamOfPlayer == Team.undefined) ;
        }

        #endregion

        /*
        void OnMouseDown()
        {
            if (teamOfPlayer == Team.theif)
                Debug.Log("Theif");
            else if (teamOfPlayer == Team.detective)
                Debug.Log("Detective");
            else
                Debug.Log("Error: Undefined");
        }
        */
    }
}
