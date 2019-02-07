using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheTheif
{
    public class MultiplayRoomManager : Photon.PunBehaviour, IPunObservable
    {
        /// <summary>
        /// The singleton of room manager
        /// </summary>
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

        //조건3. 게임이 시작하고 종료될 때 모든 플레이어를 통제할 수 있어야 함. RPC를 통해서 구현 가능할 듯
        //Player ready-check flag array
        private List<bool> isPlayersReady;

        MapDataManager mapDataManager;

        [SerializeField]
        private int thievesNum;

        static readonly string sceneLoadedKey = "Scene Loaded";
        static readonly string gameInitKey = "Game Initiated";
        static readonly string readyKey = "Ready";

        [SerializeField]
        private string levelName;

        private void Awake()
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            PhotonNetwork.sendRate = 10;
            PhotonNetwork.sendRateOnSerialize = 10;

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

            // Set the player's status as not ready
            PhotonExtends.SetLocalPlayerPropsByElem(readyKey, false);

            if (PhotonNetwork.isMasterClient) {
                //Randomly switch the master client. It prevents that the player who made room always be picked as a thief.
                int[] randomPlayerSelector = new int[PhotonNetwork.playerList.Length];
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                    randomPlayerSelector[i] = PhotonNetwork.playerList[i].ID;

                Globals.RandomizeArray<int>(randomPlayerSelector);

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
        /// Should be called when reset entire game.
        /// 
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

            Globals.RandomizeArray<int>(thiefSelector);

            // Select thieves(master client + others)
            roomCp[TeamKey(PhotonNetwork.player.ID)] = (int)Team.Thief;
            for (int i = 0; i < thievesNum - 1; i++)
            {
                PhotonPlayer thiefPlayer = PhotonExtends.GetPlayerByID(thiefSelector[i]);
                roomCp[TeamKey(thiefPlayer.ID)] = (int)Team.Thief;
            }

            Debug.Log("We load the " + levelName);
            //Load the game level. Use LoadLevel to synchronize(automaticallySyncScene is true)
            PhotonNetwork.LoadLevel(levelName);

            roomCp[sceneLoadedKey] = true;
            PhotonNetwork.room.SetCustomProperties(roomCp);
        }

        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != SceneManager.GetSceneByName(levelName))
            {
                Debug.LogError("A wrong scene is loaded. Scene name: " + SceneManager.GetActiveScene().name);
                return;
            }

            mapDataManager = MapDataManager.Instance;

            //Initialize variables related to items
            itemNum = itemPrefabs.Length;
            itemGenPointNum = mapDataManager.ItemGenPoints.Count;
            isItemStolen = new bool[itemGenPointNum];

            //Initilaize team information and set UI informations
            myTeam = (Team)PhotonNetwork.room.CustomProperties[TeamKey(PhotonNetwork.player.ID)];
            UIManager.Instance.SetTeamLabel(myTeam);
            UIManager.Instance.RenewThievesNum(thievesNum);

            //Game initiation by the master client.
            if (PhotonNetwork.isMasterClient)
            {
                GenerateGameObjects();
                SetStartTime();
            }
        }

        private void Start()
        {
            prevTimeStamp = PhotonNetwork.ServerTimestamp;
        }

        /// <summary>
        /// Generate NPCs and Items for current game. Should be called by the master client.
        /// </summary>
        private void GenerateGameObjects()
        {
            if (!PhotonNetwork.isMasterClient) {
                Debug.LogError("Object generation must be oprated by the master client.");
                return;
            }

            //Generate NPCs
            NPCGeneration(10);

            //Generate Items
            GenerateItems();

            PhotonExtends.SetRoomCustomPropsByElem(gameInitKey, true);
        }

        public GameObject NPCPrefab;
        public Transform NPCParent;
        /// <summary>
        /// Generate NPCs at random nodes.
        /// </summary>
        /// <param name="NPCNum">number of NPCs to be generated</param>
        private void NPCGeneration(int NPCNum)
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
                newNPC.transform.parent = NPCParent;
            }
        }

        public GameObject[] itemPrefabs;
        public Transform itemParent;
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
        private void GenerateItems()
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

            Globals.RandomizeArray<int>(itemNumInGenPoint);

            // Select items that theives should steal.
            int[] targetItemPointSelector = new int[itemGenPointNum];
            for (int i = 0; i < itemGenPointNum; i++)
                targetItemPointSelector[i] = i;

            Globals.RandomizeArray<int>(targetItemPointSelector);

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
                newItem.transform.parent = itemParent;
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
        private void SetTargetItemList(int[] targetPoints)
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

        /// <summary>
        /// Wait time for other user's game-ready(in ms).
        /// </summary>
        [SerializeField]
        private int readyWait = 3000;

        /// <summary>
        /// Set start time and send it to other players. Should be called by the master client.
        /// </summary>
        private void SetStartTime()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                Debug.LogError("Start time must be set by the master client.");
                return;
            }

            int curTimeStamp = PhotonNetwork.ServerTimestamp;
            int startTimeStamp = curTimeStamp + readyWait;

            bool ifOverFlow = false;
            if (startTimeStamp < curTimeStamp)
                ifOverFlow = true;

            photonView.RPC("CheckStartTimeAndReady", PhotonTargets.All, curTimeStamp, startTimeStamp, ifOverFlow);
        }

        // Start/End time of the game(use server time to set)
        [SerializeField]
        private int timeStampGone;
        [SerializeField]
        bool inReady = false;

        //issue: 애초에 마스터부터가 inready상태에 안들어가면 아무 동작도 안되고 그냥 게임이 헛돌기만 함.
        //조건도 잘못되서 안맞는듯하고, 그렇게 되는 상황을 고려할 필요가 있음(SetStartTime을 다시돌리는등)
        [PunRPC]
        private void CheckStartTimeAndReady(int sentTimeStamp, int startTimeStamp, bool ifOverFlow)
        {
            int curTimeStamp = PhotonNetwork.ServerTimestamp;
            if ((ifOverFlow && (curTimeStamp > sentTimeStamp || curTimeStamp < startTimeStamp))
                || (!ifOverFlow && (curTimeStamp > sentTimeStamp && curTimeStamp < startTimeStamp)))
            {
                timeStampGone = curTimeStamp - startTimeStamp;
                PhotonExtends.SetLocalPlayerPropsByElem(readyKey, true);
                inReady = true;
            }
        }

        private int prevTimeStamp;
        private void Update()
        {
            if (inReady)
            {
                timeStampGone += PhotonNetwork.ServerTimestamp - prevTimeStamp;
                if (timeStampGone > 0f)
                {
                    bool playerIsReady = true;
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        playerIsReady = (bool)player.CustomProperties[readyKey];
                        if (!playerIsReady)
                        {
                            PhotonExtends.SetLocalPlayerPropsByElem(readyKey, false);
                            inReady = false;
                            if (PhotonNetwork.isMasterClient)
                                SetStartTime();
                            break;
                        }
                    }

                    if (playerIsReady)
                        Debug.Log("Game Start!");
                }
            }

            prevTimeStamp = PhotonNetwork.ServerTimestamp;
        }

        //issue point
        // 현재 게임 도중에 나가도 실행되는 버그가 있음
        // 정확히는 처음에 마스터 클라이언트를 바꾸었을 경우 InitRoomAndLoadScene()를 실행하는 과정에서 ifGameInited가 싱크가 안됨
        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            if (newMasterClient != PhotonNetwork.player)
                return;

            Debug.Log("Assgined as the new master client.");
            if (PhotonNetwork.room.CustomProperties[sceneLoadedKey] == null)
                InitRoomAndLoadScene();
            else if (PhotonNetwork.room.CustomProperties[gameInitKey] == null)
                GenerateGameObjects();
        }

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

        }

        public override void OnLeftRoom()
        {
            // Remove scene load delegate
            SceneManager.sceneLoaded -= OnGameSceneLoaded;
            Destroy(this);
        }

        string TeamKey(int id)
        {
            return "Team of Player " + id;
        }
    }
}
