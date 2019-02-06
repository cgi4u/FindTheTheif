using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
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

        private Team myTeam;
        public Team MyTeam
        {
            get
            {
                return myTeam;
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

        [SerializeField]
        private int thievesNum;

        /// <summary>
        /// Used to check if the game initialization ends or not.
        /// </summary>
        [SerializeField]
        private bool ifGameInited = false;

        [SerializeField]
        private string levelName;

        void Awake()
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            DontDestroyOnLoad(this);

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

            //Add game initilization function as delegate of scene loading
            SceneManager.sceneLoaded += OnGameSceneLoaded;

            //Get room custom properties
            Hashtable roomCp = PhotonNetwork.room.CustomProperties;
            if (!int.TryParse(roomCp["Thieves Number"].ToString(), out thievesNum))
            {
                Debug.LogError("Thieves number(in custom property) is not set properly.");
                return;
            }

            if (PhotonNetwork.isMasterClient) {
                //Randomly switch the master client. It prevents that the player who made room always be picked as a thief.
                int[] randomPlayerSelector = new int[PhotonNetwork.playerList.Length];
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                    randomPlayerSelector[i] = PhotonNetwork.playerList[i].ID;

                GlobalFunctions.RandomizeArray<int>(randomPlayerSelector);

                if (randomPlayerSelector[0] == PhotonNetwork.player.ID)
                    InitRoomAndLoadScene();
                else
                {
                    Debug.Log("Change the master client.");

                    PhotonPlayer newMaster = null;
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        if (randomPlayerSelector[0] == player.ID)
                        {
                            newMaster = player;
                            break;
                        }
                    }

                    //issue: null 오류처리 필요
                    PhotonNetwork.SetMasterClient(newMaster);
                }
            }
        }

        /// <summary>
        /// Select theives player randomly and load scene.
        /// Should be called only by the master client. The master client must not be a detective because of sync delay issue.
        /// </summary>
        private void InitRoomAndLoadScene()
        {
            int playerNum = PhotonNetwork.playerList.Length;
            if (playerNum <= thievesNum)
            {
                Debug.LogError("Not enough player for game(Should be more than " + thievesNum + " players(number of thieves).");
                return;
            }

            // Set each player's team
            Hashtable roomCp = PhotonNetwork.room.CustomProperties;

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
                roomCp[TeamKey(player.ID)] = (int)Team.Detective;

            // Choose theif players randomly
            int[] thiefSelector = new int[playerNum - 1];
            int offset = 0;
            for (int i = 0; i < playerNum; i++)
            {
                // Because the master client must be a thief, not picked as a random thief player
                if (PhotonNetwork.playerList[i].ID == PhotonNetwork.player.ID)
                {
                    offset = 1;
                    continue;
                }

                thiefSelector[i - offset] = PhotonNetwork.playerList[i].ID;
            }

            GlobalFunctions.RandomizeArray<int>(thiefSelector);

            // Select thieves(master client + others)
            roomCp[TeamKey(PhotonNetwork.player.ID)] = (int)Team.Thief;
            for (int i = 0; i < thievesNum - 1; i++)
            {
                PhotonPlayer thiefPlayer = GlobalFunctions.GetPlayerByID(thiefSelector[i]);
                roomCp[TeamKey(thiefPlayer.ID)] = (int)Team.Thief;
            }

            PhotonNetwork.room.SetCustomProperties(roomCp);

            Debug.Log("We load the " + levelName);
            //Load the game level. Use LoadLevel to synchronize(automaticallySyncScene is true)
            PhotonNetwork.LoadLevel(levelName);

            ifGameInited = true;
        }

        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != SceneManager.GetSceneByName(levelName))
            {
                Debug.LogError("A wrong scene is loaded. Scene name: " + SceneManager.GetActiveScene().name);
                return;
            }

            // Remove scene load delegate
            SceneManager.sceneLoaded -= OnGameSceneLoaded;

            mapDataManager = MapDataManager.Instance;

            //Initialize variables related to items
            itemNum = itemPrefabs.Length;
            itemGenPointNum = mapDataManager.ItemGenPoints.Count;
            isItemStolen = new bool[itemGenPointNum];

            //Initilaize team information and set UI informations
            myTeam = (Team)PhotonNetwork.room.CustomProperties[TeamKey(PhotonNetwork.player.ID)];
            UIManager.Instance.SetTeamLabel(myTeam);
            UIManager.Instance.RenewThievesNum(thievesNum);

            //Generate NPCs and Items
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
            //issue: 다른 플레이어들이 게임 준비가 됐는지를 확인 후 시작해야하고, 시간 체크도 그 이후부터 시작해야함.

            //Reduce spent time
            if (PhotonNetwork.isMasterClient && timeLeft - Time.deltaTime > 0.0f)
            {
                timeLeft -= Time.deltaTime;
            }
            //TODO: Game End 처리하기. 일단 메소드 만들고 RPC화
        }

        public GameObject NPCPrefab;
        void NPCGeneration(int NPCNum)
        {
            for (int i = 0; i < NPCNum; i++)
            {
                int randomPoint = mapDataManager.GetRandomNPCGenPoint();
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

            GlobalFunctions.RandomizeArray<int>(itemNumInGenPoint);

            // Select items that theives should steal.
            int[] targetItemPointSelector = new int[itemGenPointNum];
            for (int i = 0; i < itemGenPointNum; i++)
                targetItemPointSelector[i] = i;

            GlobalFunctions.RandomizeArray<int>(targetItemPointSelector);

            int[] targetPoints = new int[targetItemNum];
            int count = 0;
            List<ExhibitRoom> roomContainTargetItem = new List<ExhibitRoom>();
            for (int i = 0; i < itemGenPointNum; i++)
            {
                //Not allow the case in which multiple items in a same room.
                ExhibitRoom roomOfPoint = mapDataManager.ItemGenPoints[targetItemPointSelector[i]].GetComponentInParent<ExhibitRoom>();
                if (!roomContainTargetItem.Contains(roomOfPoint))
                {
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

                ExhibitRoom roomOfItem = mapDataManager.ItemGenPoints[i].GetComponentInParent<ExhibitRoom>();
                PhotonView.Get(newItem).RPC("Init", PhotonTargets.All, roomOfItem.Floor, mapDataManager.Rooms.FindIndex(room => room == roomOfItem), i);
            }

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if ((Team)PhotonNetwork.room.CustomProperties[TeamKey(player.ID)] == Team.Thief)
                    photonView.RPC("SetTargetItemList", player, targetPoints);
            }
        }

        /// <summary>
        /// Set target items which the player should be steal(For a thief player).
        /// </summary>
        /// <param name="targetPoints">Indices of item generation points which have target items.</param>
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
            UIManager.Instance.RenewTargetItemList(targetItems, targetItems.Count, null);
        }

        //issue point
        // 현재 게임 도중에 나가도 실행되는 버그가 있음
        // 정확히는 처음에 마스터 클라이언트를 바꾸었을 경우 InitRoomAndLoadScene()를 실행하는 과정에서 ifGameInited가 싱크가 안됨
        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (newMasterClient != PhotonNetwork.player)
                return;

            Debug.Log("Assgined to the new master client.");
            if (!ifGameInited)
                InitRoomAndLoadScene();
        }

        /*public override void OnOwnershipTransfered(object[] viewAndPlayers)
        {
            if (!ifGameInited && PhotonNetwork.isMasterClient)
            {
                InitRoomAndLoadScene();
            }
        }*/

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            Hashtable leftPlayerCp = otherPlayer.CustomProperties;

            if ((Team)PhotonNetwork.room.CustomProperties[TeamKey(otherPlayer.ID)] == Team.Thief)
            {
                thievesNum -= 1;
                UIManager.Instance.RenewThievesNum(thievesNum);
            }
            CheckIfGameEnds();
        }

        public void CheckIfGameEnds()
        {
            if (thievesNum == 0)
                Debug.Log("Game Set. Detectives win.");
            else if (PhotonNetwork.playerList.Length == thievesNum)
                Debug.Log("Game Set. Thieves win.");
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.isWriting)
            {
                stream.SendNext(ifGameInited);
                stream.SendNext(timeLeft);
            }
            else
            {
                ifGameInited = (bool)stream.ReceiveNext();
                timeLeft = (float)stream.ReceiveNext();
            }
        }

        public override void OnLeftRoom()
        {
            Destroy(this);
        }

        string TeamKey(int id)
        {
            return "Team of Player " + id;
        }
    }
}
