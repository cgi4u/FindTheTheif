using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheTheif
{
    public class MultiplayRoomManager : Photon.PunBehaviour, IPunObservable
    {
        //TODO: 룸 하나에서 전체가 공유해야 하는 데이터와 동작들을 관리
        //조건1. 권한을 주고 받을 수 있어야 함. OnRequest 구현

        //Singleton
        private static MultiplayRoomManager instance;
        public static MultiplayRoomManager Instance
        {
            get
            {
                return instance;
            }
        }

        //데이터
        // 1. 방 내 현재 남은 도둑의 수
        private int remainingThief;
        public int RemainingTheif
        {
            get
            {
                return remainingThief;
            }
        }

        // 2. 게임 진행 시간
        // ISSUE: 시간은 각 플레이어가 갱신하는게 아니라, 소유자 한명만 갱신하고 그걸 뿌려줘야함
        //          안그러면 각자 게임이 따로따로 끝나는 참사가 생길수도있음
        public float timeLeft;    //나중에 private으로

        //조건3. 게임이 시작하고 종료될 때 모든 플레이어를 통제할 수 있어야 함. RPC를 통해서 구현 가능할 듯
        //Player ready-check flag array
        private List<bool> isPlayersReady;

        MapDataManager mapDataManager;

        void Awake()
        {
            mapDataManager = MapDataManager.Instance;

            //Initialize variables related to items
            itemNum = itemPrefabs.Length;
            itemGenPointNum = mapDataManager.ItemGenPoints.Count;
            isItemStolen = new bool[itemGenPointNum];

            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            //Set the singleton
            if (instance == null)
            {
                //Debug.Log("Room Manager Instantiation");
                instance = this;
            }
            else
            {
                Debug.LogError("Multiple instantiation of the room controller");
            }

            //Get values sent by Lobby
            Hashtable roomCp = PhotonNetwork.room.CustomProperties;
            int.TryParse(roomCp["Theif Number"].ToString(), out remainingThief);

            //Set all players' ready-check flag to false
            int playerNum = PhotonNetwork.playerList.Length;
            isPlayersReady = new List<bool>();
            for (int i = 0; i < playerNum; i++)
                isPlayersReady.Add(false);
            
        }

        void Start()
        {
            if (PhotonNetwork.isMasterClient)
            {
                //Generate NPCs
                NPCGeneration(10);

                //Generate Items
                GenerateItems();
            }
        }

        void Update()
        {
            //Reduce spent time
            if (timeLeft - Time.deltaTime > 0.0f)
            {
                timeLeft -= Time.deltaTime;
            }
            //TODO: Game End 처리하기. 일단 메소드 만들고 RPC화
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(remainingThief);
            }
            else
            {
                remainingThief = (int)stream.ReceiveNext();
            }
        }

        public GameObject NPCPrefab;
        void NPCGeneration(int NPCNum)
        {
            for (int i = 0; i < NPCNum; i++)
            {
                int randomPoint = MapDataManager.Instance.GetRandomNPCGenPoint();
                if (randomPoint == -1)
                {
                    Debug.LogError("Error: Attempt to generate more number of NPC than available");
                    return;
                }

                GameObject newNPC = PhotonNetwork.InstantiateSceneObject(NPCPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0, null);
                PhotonView.Get(newNPC).RPC("Init", PhotonTargets.All, randomPoint);
            }
        }

        public GameObject[] itemPrefabs;
        [SerializeField]
        private int itemNum;                    // The number of item prefabs = The number of kind of items which can be generated

        [SerializeField]
        List<ItemController> itemInGenPoint = new List<ItemController>();
        [SerializeField]
        private int itemGenPointNum;            // The number of item generation points = The number of items generated in a game room
        bool[] isItemStolen;                    // 아이템이 훔쳐졌는가 여부

        [SerializeField]
        private int targetItemNum;                // 훔쳐야 할 아이템의 개수
        [SerializeField]
        private List<ItemController> targetItems = new List<ItemController>();     // 훔쳐야 할 아이템의 리스트
        public List<ItemController> TargetItems
        {
            get
            {
                return targetItems;
            }
        }

        /// <summary>
        /// Generate items at generation points randomly. 
        /// </summary>
        void GenerateItems()
        {
            if (itemGenPointNum > itemNum)
            {
                Debug.LogError("There are fewer items than generation points.");
                return;
            }
            if (itemNum < targetItemNum)
            {
                Debug.LogError("The number of items to steal can't exceed the number of all items.");
                return;
            }

            // Select item for each generation point randomly.
            int[] itemNumInGenPoint = new int[itemNum];
            for (int i = 0; i < itemNum; i++)
                itemNumInGenPoint[i] = i;

            for (int i = 0; i < itemNum * 3; i++)
            {
                int r1 = Random.Range(0, itemNum);
                int r2 = Random.Range(0, itemNum);

                int temp = itemNumInGenPoint[r1];
                itemNumInGenPoint[r1] = itemNumInGenPoint[r2];
                itemNumInGenPoint[r2] = temp;
            }

            // Select items that theives should steal.
            int[] targetItemPointSelector = new int[itemGenPointNum];
            for (int i = 0; i < itemGenPointNum; i++)
                targetItemPointSelector[i] = i;

            for (int i = 0; i < itemGenPointNum * 3; i++)
            {
                int r1 = Random.Range(0, itemGenPointNum);
                int r2 = Random.Range(0, itemGenPointNum);

                int temp = targetItemPointSelector[r1];
                targetItemPointSelector[r1] = targetItemPointSelector[r2];
                targetItemPointSelector[r2] = temp;
            }

            int[] targetPoints = new int[targetItemNum];
            int count = 0;
            List<ExhibitRoom> roomContainTargetItem = new List<ExhibitRoom>();
            for (int i = 0; i < itemGenPointNum; i++)
            {
                //Not allow the case in which multiple items in a same room.
                ExhibitRoom roomOfPoint = mapDataManager.ItemGenPoints[targetItemPointSelector[i]].GetComponentInParent<ExhibitRoom>();
                if (!roomContainTargetItem.Contains(roomOfPoint))
                {
                    //Debug.Log("Count: " + count);
                    //Debug.Log("i: " + i);
                    targetPoints[count++] = targetItemPointSelector[i];
                    roomContainTargetItem.Add(roomOfPoint);
                    if (count == targetItemNum)
                        break;
                }
            }

            if (count != targetItemNum)
            {
                Debug.LogError("There are not enough rooms for item generation.");
                return;
            }

            for (int i = 0; i < itemGenPointNum; i++)
            {
                GameObject newItem = PhotonNetwork.InstantiateSceneObject("Items\\" + itemPrefabs[itemNumInGenPoint[i]].name, 
                                                                    mapDataManager.ItemGenPoints[i].transform.position, Quaternion.identity, 0, null);

                bool isTarget = targetPoints.Contains(i);
                ExhibitRoom roomOfItem = mapDataManager.ItemGenPoints[i].GetComponentInParent<ExhibitRoom>();
                PhotonView.Get(newItem).RPC("Init", PhotonTargets.All, roomOfItem.Floor, mapDataManager.Rooms.FindIndex(room => room == roomOfItem), i);
            }
            photonView.RPC("SetTargetItemList", PhotonTargets.All, targetPoints);

            //photonView.RPC("InitItemSetting", PhotonTargets.All, itemNumInGenPoint, targetItemPointSelector);
        }

        /// <summary>
        /// Send the result of random select in master client to other players
        /// </summary>
        /// <param name="itemNumInGenPoint">Item Prefab index of item that generated at point of same index</param>
        /// <param name="stealItemSelector">Randomize result for steal targets</param>
        [PunRPC]
        void InitItemSetting(int[] itemNumInGenPoint, int[] stealItemSelector)
        {


            //Prefab은 실제 게임 내에 존재하는 오브젝트가 아님. 실제 Instantiate한 오브젝트를 참조해서 steal target item 리스트에 넣어야함.
            //이것을 위해 target item만 갱신하는 rpc를 따로 넣어야할수도 있음.
            for (int i = 0; i < targetItemNum; i++)
            {
                ItemController newTargetItem = itemPrefabs[stealItemSelector[i]].GetComponent<ItemController>();
                if (itemInGenPoint.Exists(item => item == newTargetItem))
                    targetItems.Add(itemPrefabs[stealItemSelector[i]].GetComponent<ItemController>());
                else
                    i--;
            }

            //UIManager.Instance.RenewStealItemList(targetItemList, targetItemNum, isItemStolen);
        }

        [PunRPC]
        void SetTargetItemList(int[] targetPoints)
        {
            for (int i = 0; i < targetPoints.Length; i++)
            {
                print(targetPoints[i]);
                Debug.Log(mapDataManager.ItemGenPoints[targetPoints[i]]);
            }

            for (int i = 0; i < targetPoints.Length; i++)
                targetItems.Add(mapDataManager.ItemGenPoints[targetPoints[i]].Item);
        }
    }
}
