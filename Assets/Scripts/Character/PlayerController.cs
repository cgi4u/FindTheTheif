using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(PhotonTransformView))]
    public class PlayerController : Photon.PunBehaviour, IPunObservable
    {
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

        private Vector2 raycastBox; // Collider of a characters

        #region Initialization(Awake, Start)

        private void Awake()
        {
            raycastBox = GetComponent<BoxCollider2D>().size;   // To ignore collisions on edges

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

        #endregion

        // Update is called once per frame
        void Update()
        {
            if (isMoving)
            {
                //GetComponent<PhotonTransformView>().SetSynchronizedValues(moveSpeed * direction, 0f);
                Move();
            }
            else if (curDirection != EMoveDirection.Stop)
                SetNewTargetPoint();

            //Set sprite sorting order by using y-axis postion
            GetComponent<SpriteRenderer>().sortingOrder = -(int)(transform.position.y * 100f);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.tag == "NPC")
                return;

            transform.position = startPoint;

            isMoving = false;
            SetAnimationProperty();
        }

        #region Player Moving

        protected Vector2 startPoint;
        protected Vector2 targetPoint;

        [SerializeField]
        private float moveSpeed;
        public float MoveSpeed
        {
            get
            {
                return moveSpeed;
            }
            set
            {
                moveSpeed = value;
            }
        }

        private bool isMoving = false;

        [SerializeField]
        private EMoveDirection curDirection = EMoveDirection.Stop;

        public void ChangeMoveDirection(EMoveDirection direction)
        {
            curDirection = direction;
        }

        public void OnMoveButtonPushed(EMoveDirection dir)
        {
            btnCount += 1;
            switch (dir)
            {
                case EMoveDirection.Up:
                    buttons[0] = btnCount;
                    break;
                case EMoveDirection.Down:
                    buttons[1] = btnCount;
                    break;
                case EMoveDirection.Left:
                    buttons[2] = btnCount;
                    break;
                case EMoveDirection.Right:
                    buttons[3] = btnCount;
                    break;
                default:
                    Debug.Log("Error: Undefined input for moving.");
                    break;
            }
        }

        public void OnMoveButtonReleased(EMoveDirection dir)
        {
            switch (dir)
            {
                case EMoveDirection.Up:
                    buttons[0] = 0;
                    break;
                case EMoveDirection.Down:
                    buttons[1] = 0;
                    break;
                case EMoveDirection.Left:
                    buttons[2] = 0;
                    break;
                case EMoveDirection.Right:
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
        }

        public int CurFloor { get; set; }

        private void Move()
        {
            //설정 속도에 따라 움직일 위치를 계산(MoveTowards) 이후 이동
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);
            //SetRatioBarValue(Vector2.Distance(transform.position, startPoint) / Vector2.Distance(startPoint, targetPoint));

            //목적지에 도달했을 경우 버튼 입력 상황에 따라 목적지를 재계산하거나 멈춤
            if (transform.position.Equals(targetPoint))
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, new Vector2());
                foreach (RaycastHit2D hit in hits)
                {
                    StairPoint stairPoint = hit.collider.gameObject.GetComponent<StairPoint>();
                    if (stairPoint != null)
                    {
                        Vector2 newPos = stairPoint.LinkedPoint;

                        bool posConfirmed = false;
                        while (!posConfirmed)
                        {
                            RaycastHit2D[] stairFrontHits = Physics2D.RaycastAll(newPos, new Vector2());

                            posConfirmed = true;
                            foreach (RaycastHit2D stairFrontHit in stairFrontHits)
                            {
                                if (!stairFrontHit.collider.isTrigger)
                                {
                                    posConfirmed = false;

                                    if (newPos.y > transform.position.y)
                                        newPos += Vector2.up;
                                    else
                                        newPos += Vector2.down;

                                    break;
                                }
                            }
                        }

                        if (stairPoint.IsToUpstair)
                            CurFloor += 1;
                        else
                            CurFloor -= 1;
                        Debug.Log(CurFloor);
                        transform.position = newPos;
                        break;
                    }

                    SecretPathController secretPath = hit.collider.gameObject.GetComponent<SecretPathController>();
                    Vector3 linkedPos;
                    if (secretPath != null && (linkedPos = secretPath.GetLinkedPos()).z == 0f)
                    {
                        transform.position = linkedPos;
                        break;
                    }
                }

                if (curDirection != EMoveDirection.Stop)
                    SetNewTargetPoint();
                else
                {
                    isMoving = false;
                    SetAnimationProperty();
                }
            }

            return;
        }

        Vector2 directionVec = new Vector2(0f, 0f);
        private void SetNewTargetPoint()
        {
            startPoint = (Vector2)transform.position;   // Set starting point

            switch (curDirection)
            {
                case EMoveDirection.Down:
                    directionVec = Vector2.down;
                    break;
                case EMoveDirection.Right:
                    directionVec = Vector2.right;
                    break;
                case EMoveDirection.Up:
                    directionVec = Vector2.up;
                    break;
                case EMoveDirection.Left:
                    directionVec = Vector2.left;
                    break;
            }
            targetPoint = startPoint + directionVec;

            //움직이는 과정에서 플레이어와 충돌하는 물체가 있을지를 판단.
            //플레이어(자기자신)의 콜라이더와 무조건 충돌하므로 다른 콜라이더가 있는지 판단하기 위해 BoxCast가 아닌 BoxCastAll을 쓴다.
            RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, targetPoint - startPoint, Vector2.Distance(startPoint, targetPoint));

            //플레이어 자기 자신 이외에 충돌 물체가 있다면 이동하지 않는다.
            bool ifHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != gameObject && !hit.collider.isTrigger)
                {
                    //플레이어 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
                    ifHit = true;
                    break;
                }
            }

            if (ifHit)
                isMoving = false;
            else
                isMoving = true;

            SetAnimationProperty();
        }

        private void SetAnimationProperty()
        {
            Animator animator = GetComponent<Animator>();

            switch (curDirection)
            {
                case EMoveDirection.Up:
                    animator.SetInteger("Direction", 0);
                    break;
                case EMoveDirection.Down:
                    animator.SetInteger("Direction", 1);
                    break;
                case EMoveDirection.Left:
                    animator.SetInteger("Direction", 2);
                    break;
                case EMoveDirection.Right:
                    animator.SetInteger("Direction", 3);
                    break;
            }
            animator.SetBool("IsMoving", isMoving);
        }

        #endregion

        /*private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log("Trigger: " + collision.gameObject.name);
        }*/

        #region Photon Synchronization

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(startPoint);
                stream.SendNext(targetPoint);
                stream.SendNext(CurFloor);
            }
            else
            {
                int oldFloor = CurFloor;

                startPoint = (Vector2)stream.ReceiveNext();
                targetPoint = (Vector2)stream.ReceiveNext();
                CurFloor = (int)stream.ReceiveNext();
            }
        }

        #endregion

        #region Move Ratio Bar

        
        public GameObject ratioBar;
        private void SetRatioBarValue(float ratio)
        {
            ratioBar.GetComponent<Image>().fillAmount = ratio;
        }


        #endregion

        #region Skill Implementation

        float oldSpeed;
        public void BoostSpeed(float magnification, int seconds)
        {
            oldSpeed = moveSpeed;
            moveSpeed = moveSpeed * magnification;
            StartCoroutine(RestoreSpeed(seconds));
        }

        private IEnumerator RestoreSpeed(int seconds)
        {
            yield return new WaitForSeconds(seconds);

            moveSpeed = oldSpeed;
        }

        #endregion
    }
}
