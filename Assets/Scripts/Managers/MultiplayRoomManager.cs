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

        void Awake()
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            //Set singleton
            //이 오류 체크는 사실 큰 필요가 없음.
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
                ItemGeneration();
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
                RouteNode randomPoint = MapDataManager.Instance.GetRandomGenerationPoint();
                if (randomPoint == null)
                {
                    Debug.LogError("Error: Attempt to generate more number of NPC than available");
                    return;
                }

                GameObject newNPC = PhotonNetwork.InstantiateSceneObject(NPCPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0, null);
                //PhotonView.Get(newNPC).RPC("ManualStart", PhotonTargets.All, randomPoint);
                newNPC.GetComponent<NPCController>().ManualStart(randomPoint);
            }
        }

        public GameObject[] itemPrefabs;
        public int itemNum;                     // 맵 상 아이템의 총 개수
        public int stealItemNum;                // 훔쳐야 할 아이템의 개수
        List<ItemController> stealItemList;     // 훔쳐야 할 아이템의 리스트
        bool[] isItemStolen;                    // 아이템이 훔쳐졌는가 여부
        void ItemGeneration()
        {
            if (itemNum < stealItemNum)
            {
                Debug.LogError("The number of items to steal can't exceed the number of all items.");
            }

            for (int i = 0; i < itemNum; i++)
            {
                int r1 = Random.Range(0, itemNum);
                int r2 = Random.Range(0, itemNum);

                GameObject temp = itemPrefabs[r1];
                itemPrefabs[r1] = itemPrefabs[r2];
                itemPrefabs[r2] = temp;
            }

            MapDataManager mapDataManager = MapDataManager.Instance;
            if (mapDataManager.ItemGenerationPoints.Count > itemNum)
            {
                Debug.LogError("There are fewer items than generation points.");
                return;
            }

            bool[] isItemToSteal = new bool[itemNum];
            int count = 0;
            while (count < stealItemNum)
            {
                int r = Random.Range(0, itemNum);
                if (isItemToSteal[r] == false)
                {
                    isItemToSteal[r] = true;
                    count++;
                }
            }

            for (int i = 0; i < mapDataManager.ItemGenerationPoints.Count; i++)
            {
                GameObject newItem = PhotonNetwork.Instantiate("Items\\" + itemPrefabs[i].name, mapDataManager.ItemGenerationPoints[i].position, Quaternion.identity, 0);
                if (isItemToSteal[i] == true)
                    stealItemList.Add(newItem.GetComponent<ItemController>());

                ExhibitRoom roomOfItem = mapDataManager.ItemGenerationPoints[i].GetComponentInParent<ExhibitRoom>();
                newItem.GetComponent<ItemController>().Init(roomOfItem.floor, roomOfItem.num);
            }
            isItemStolen = new bool[stealItemNum];
            UIManager.Instance.RenewStealItemList(stealItemList, stealItemNum, isItemStolen);
        }
    }
}
