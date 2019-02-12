using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheThief
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

        MapDataManager mapDataManager;

        #region Keys for property setting

        static readonly string sceneLoadedKey = "Scene Loaded";
        static readonly string gameInitKey = "Game Initiated";
        static readonly string readyKey = "Ready";
        static readonly string startKey = "Game Started";

        private string TeamKey(int id)
        {
            return "Team of Player " + id;
        }
        static readonly string pauseKey = "Pause";

        #endregion

        #region Room Initialization(Before Loading Room)

        private int thievesNum;
        private void Awake()
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            // Modify PhotonNetwork settings according to in-game mode.
            PhotonNetwork.BackgroundTimeout = 30f;
            //PhotonNetwork.sendRate = 10;
            //PhotonNetwork.sendRateOnSerialize = 10;

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

                Globals.RandomizeArray<int>(randomPlayerSelector);

                if (randomPlayerSelector[0] == PhotonNetwork.player.ID)
                    InitRoomAndLoadScene();
                else
                {
                    Debug.Log("Change the master client.");

                    for (int i = 0; i < randomPlayerSelector.Length; i++)
                    {
                        PhotonPlayer newMaster = PhotonPlayer.Find(randomPlayerSelector[i]);
                        if (newMaster != null)
                        {
                            PhotonNetwork.SetMasterClient(newMaster);
                            break;
                        }
                    }
                }
            }
        }

        public string roomLevelName;
        /// <summary>
        /// Select theives player randomly and load scene.
        /// Should be called only by the master client. The master client must not be a detective because of sync delay issue.
        /// Should be called when reset entire game.
        /// </summary>
        private void InitRoomAndLoadScene()
        {
            int playerNum = PhotonNetwork.playerList.Length;
            if (playerNum <= thievesNum)
            {
                Debug.LogError("Not enough player for game(Should be more than " + thievesNum + " players(number of thieves).");
                return;
            }

            // Set player status in room(pause, ready)
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                Hashtable playerCp = player.CustomProperties;
                playerCp[pauseKey] = false;
                playerCp[readyKey] = false;
                player.SetCustomProperties(playerCp);
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
                PhotonPlayer thiefPlayer = PhotonPlayer.Find(thiefSelector[i]);
                roomCp[TeamKey(thiefPlayer.ID)] = (int)Team.Thief;
            }

            Debug.Log("We load the " + roomLevelName);
            //Load the game level. Use LoadLevel to synchronize(automaticallySyncScene is true)
            PhotonNetwork.LoadLevel(roomLevelName);

            roomCp[sceneLoadedKey] = true;
            PhotonNetwork.room.SetCustomProperties(roomCp);
        }

        #endregion

        #region Game Initialization(After Loading Room, Including Object Generation) 

        private Team myTeam;
        /// <summary>
        /// Team of the local player.
        /// </summary>
        public Team MyTeam
        {
            get
            {
                return myTeam;
            }
        }

        List<PhotonPlayer> detectives = new List<PhotonPlayer>();
        List<PhotonPlayer> theives = new List<PhotonPlayer>();
        List<int> masterPriority = new List<int>();
        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != SceneManager.GetSceneByName(roomLevelName))
            {
                Debug.LogError("A wrong scene is loaded. Scene name: " + SceneManager.GetActiveScene().name);
                return;
            }

            mapDataManager = MapDataManager.Instance;

            //Initilaize team information and set UI informations
            myTeam = (Team)PhotonNetwork.room.CustomProperties[TeamKey(PhotonNetwork.player.ID)];
            UIManager.Instance.SetTeamLabel(myTeam);
            UIManager.Instance.RenewThievesNum(thievesNum);

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                Team teamOfPlayer = (Team)PhotonNetwork.room.CustomProperties[TeamKey(player.ID)];
                if (teamOfPlayer == Team.Detective)
                {
                    detectives.Add(player);
                }
                else if (teamOfPlayer == Team.Thief)
                {
                    theives.Add(player);
                }
            }

            // Set master client priority  
            foreach (PhotonPlayer theifPlayer in theives)
                masterPriority.Add(theifPlayer.ID);
            masterPriority.Sort();

            List<int> detectivePriority = new List<int>();
            foreach (PhotonPlayer detectivePlayer in theives)
                detectivePriority.Add(detectivePlayer.ID);
            detectivePriority.Sort();

            masterPriority.AddRange(detectivePriority);


            // Game initiation by the master client.
            if (PhotonNetwork.isMasterClient)
            {
                GameInitByMaster();
            }
        }
        
        /// <summary>
        /// Generate NPCs and Items for current game. Should be called by the master client.
        /// </summary>
        private void GameInitByMaster()
        {
            if (!PhotonNetwork.isMasterClient) {
                Debug.LogError("Object generation must be oprated by the master client.");
                return;
            }

            // Generate Scene Objects(NPCs, Items).
            NPCGeneration();
            GenerateItems();

            // Game initiation ends, send ready signal to other players. 
            PhotonExtends.SetRoomCustomPropsByElem(gameInitKey, true);
            photonView.RPC("SetPlayerReady", PhotonTargets.All);
        }

        public int numberOfNPC;
        public GameObject NPCPrefab;
        public Transform NPCParent;
        /// <summary>
        /// Generate NPCs at random nodes.
        /// </summary>
        private void NPCGeneration()
        {
            for (int i = 0; i < numberOfNPC; i++)
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
        public int numOfTargetItem; 
        private List<ItemController> targetItems = new List<ItemController>();
        /// <summary>
        /// Generate items at generation points randomly. 
        /// </summary>
        private void GenerateItems()
        {
            // Initialize variables related to items.
            int numOfItems = itemPrefabs.Length;
            int numOfItemGenPoint = mapDataManager.ItemGenPoints.Count;
            if (numOfItemGenPoint > numOfItems)
            {
                Debug.LogError("There are fewer items than generation points.");
                return;
            }
            if (numOfItemGenPoint < numOfTargetItem)
            {
                Debug.LogError("The number of items to steal can't exceed the number of all items in the level.");
                return;
            }

            // Select the target item property and divide the item prefab list 
            // to the group which has that property and the group which doesn't have.
            // issue: 카테고리 및 속성 번호를 수동으로 지정, 이후 enum 활용식 혹은 다른 방법 고안 필요.
            int category = Random.Range(0, 3);
            int selectedProp = Random.Range(0, 3);

            switch (category)
            {
                case 0:
                    Debug.Log("Selected property: " + (ItemColor)selectedProp);
                    break;
                case 1:
                    Debug.Log("Selected property: " + (ItemAge)selectedProp);
                    break;
                case 2:
                    Debug.Log("Selected property: " + (ItemUsage)selectedProp);
                    break;
            }

            List<GameObject> ItemsHaveProp = new List<GameObject>();
            foreach (GameObject item in itemPrefabs)
            {
                ItemController itemController = item.GetComponent<ItemController>();
                //issue: Error
                if (itemController == null)
                {
                    Debug.LogError("The prefab name " + item.name + " in item list doesn't have ItemContorller.");
                    return;
                }

                switch (category)
                {
                    case 0:
                        if (itemController.Color == (ItemColor)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                    case 1:
                        if (itemController.Age == (ItemAge)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                    case 2:
                        if (itemController.Usage == (ItemUsage)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                }
            }

            // Select steal target/non-target items to generate; 
            Globals.RandomizeList<GameObject>(ItemsHaveProp);
            List<GameObject> targetItems = new List<GameObject>();
            for (int i = 0; i < numOfTargetItem; i++)
                targetItems.Add(ItemsHaveProp[i]);  // issue: 범위 에러

            List<GameObject> nonTargetItems = new List<GameObject>();
            for (int i = 0; i < numOfItems; i++)
            {
                if (!targetItems.Contains(itemPrefabs[i]))
                    nonTargetItems.Add(itemPrefabs[i]);
            }
            Globals.RandomizeList<GameObject>(nonTargetItems);

            // Select generation points where the target items are generated.
            int[] targetItemPointSelector = new int[numOfItemGenPoint];
            for (int i = 0; i < numOfItemGenPoint; i++)
                targetItemPointSelector[i] = i;
            Globals.RandomizeArray<int>(targetItemPointSelector);

            List<int> targetItemPoints = new List<int>();
            List<ExhibitRoom> roomContainTargetItem = new List<ExhibitRoom>();
            for (int i = 0; i < numOfItemGenPoint; i++)
            {
                //Not allow the case in which multiple items in a same room.
                ExhibitRoom roomOfPoint = mapDataManager.ItemGenPoints[targetItemPointSelector[i]].GetComponentInParent<ExhibitRoom>();
                if (!roomContainTargetItem.Contains(roomOfPoint))
                {
                    targetItemPoints.Add(targetItemPointSelector[i]);
                    roomContainTargetItem.Add(roomOfPoint);
                    if (targetItemPoints.Count == numOfTargetItem)
                        break;
                }
            }
            if (targetItemPoints.Count != numOfTargetItem)
            {
                //issue: Error
                Debug.LogError("There are not enough rooms for item generation.");
                return;
            }

            // Generate items using setting.
            for (int i = 0; i < numOfItemGenPoint; i++)
            {
                GameObject newItemPrefab;
                if (targetItemPoints.Contains(i))
                {
                    newItemPrefab = targetItems[0];
                    targetItems.RemoveAt(0);
                }
                else
                {
                    newItemPrefab = nonTargetItems[0];
                    nonTargetItems.RemoveAt(0);
                }

                GameObject newItem = PhotonNetwork.InstantiateSceneObject("Items\\" + newItemPrefab.name, 
                                        mapDataManager.ItemGenPoints[i].transform.position, Quaternion.identity, 0, null);
                PhotonView.Get(newItem).RPC("Init", PhotonTargets.All, i);
                newItem.transform.parent = itemParent;
            }

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                if ((Team)PhotonNetwork.room.CustomProperties[TeamKey(player.ID)] == Team.Thief)
                    photonView.RPC("SetTargetItemList", player, targetItemPoints);
            }
        }

        /// <summary>
        /// Set target items which the player should be steal(For a thief player).
        /// </summary>
        /// <param name="targetItemPoints">Indices of item generation points which have target items.</param>
        [PunRPC]
        private void SetTargetItemList(List<int> targetItemPoints)
        {
            for (int i = 0; i < targetItemPoints.Count; i++)
            {
                print(targetItemPoints[i]);
                Debug.Log(mapDataManager.ItemGenPoints[targetItemPoints[i]]);
            }

            for (int i = 0; i < targetItemPoints.Count; i++)
                targetItems.Add(mapDataManager.ItemGenPoints[targetItemPoints[i]].Item);
            UIManager.Instance.RenewTargetItemList(targetItems, targetItems.Count, null);
        }

        // Start/End time of the game(use server time to set)
        [SerializeField]
        private int leftTimeStamp;
        [SerializeField]
        bool inReady = false;

        //issue: 처음 시작시 일정 시간동안 모든플레이어의 ready상태가 만족되지 않으면 오류처리를 해주는 기능이 필요함
        [PunRPC]
        private void SetPlayerReady()
        {
            PhotonExtends.SetLocalPlayerPropsByElem(readyKey, true);
            inReady = true;
        }

        //issue: 룸은 game start상태인데 나는 아니면 회복해주는 기능이 필요함
        [PunRPC]
        private void LetGameStart()
        {
            inReady = false;
            gameStarted = true;
        }

        #endregion

        [SerializeField]
        bool gameStarted = false;
        int prevTimeStamp;
        private void Start()
        {
            prevTimeStamp = PhotonNetwork.ServerTimestamp;
        }

        int readyWaitTimeStamp = 0;
        private void Update()
        {
            int curTimeStamp = PhotonNetwork.ServerTimestamp;
            int deltaTimeStamp = curTimeStamp - prevTimeStamp;
            prevTimeStamp = curTimeStamp;

            if (inReady && PhotonNetwork.isMasterClient)
            {
                bool areAllPlayersReady = true;
                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    areAllPlayersReady = (bool)player.CustomProperties[readyKey];
                    if (!areAllPlayersReady)
                    {
                        readyWaitTimeStamp += deltaTimeStamp;
                        break;
                    }
                }

                //if all player is in ready, fire game start signal to other players(via server in order to reduce delay).
                if (areAllPlayersReady)
                {
                    PhotonExtends.SetRoomCustomPropsByElem(startKey, true);
                    photonView.RPC("LetGameStart", PhotonTargets.AllViaServer);
                }
            }

            if (!gameStarted)
                return;
            // Section after game is started

            leftTimeStamp -= deltaTimeStamp;
            UIManager.Instance.RenewTimeLabel(leftTimeStamp / 1000);
        }

        private void LateUpdate()
        {
            if (!gameStarted)
                return;
            // Section after game is started

            // If the local player is the master and in detective team, change the master client to be a theif if it is possible.
            if (PhotonNetwork.isMasterClient && myTeam == Team.Detective)
            {
                foreach (PhotonPlayer thiefPlayer in theives)
                {
                    if ((bool)thiefPlayer.CustomProperties[pauseKey] == false)
                    {
                        photonView.RPC("TakeOverMaster", thiefPlayer, PhotonNetwork.ServerTimestamp);
                        break;
                    }
                }
            }

            // If master client is paused, the player who has higest priority becomes new master client.
            if ((bool)PhotonNetwork.masterClient.CustomProperties[pauseKey] == true)
            {
                int newMasterID = PhotonNetwork.player.ID;
                for (int i = 0; i < masterPriority.Count; i++)
                {
                    bool isPlayerPaused = (bool)PhotonPlayer.Find(masterPriority[i]).CustomProperties[pauseKey];
                    if (!isPlayerPaused)
                    {
                        newMasterID = masterPriority[i];
                        break;
                    }
                }

                if (newMasterID == PhotonNetwork.player.ID)
                    PhotonNetwork.SetMasterClient(PhotonNetwork.player);
            }
        }

        /// <summary>
        /// Takeover master client from other client. Used to change the master client from detective to theif player.
        /// </summary>
        /// <param name="sentTimestamp">Timestamp when RPC was fired. If timestamp interval is too long, it is considered as timeout(default = 5000ms).</param>
        [PunRPC]
        private void TakeOverMaster(int sentTimestamp)
        {
            if (!PhotonNetwork.isMasterClient && PhotonNetwork.ServerTimestamp - sentTimestamp < 5000)
                PhotonNetwork.SetMasterClient(PhotonNetwork.player);
        }

        //issue point
        // 현재 게임 도중에 나가도 실행되는 버그가 있음
        // 정확히는 처음에 마스터 클라이언트를 바꾸었을 경우 InitRoomAndLoadScene()를 실행하는 과정에서 ifGameInited가 싱크가 안됨
        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            Debug.Log("Master Client switched.");
            if (newMasterClient != PhotonNetwork.player)
                return;

            Debug.Log("Assgined as the new master client.");
            if (PhotonNetwork.room.CustomProperties[sceneLoadedKey] == null)
                InitRoomAndLoadScene();
            else if (PhotonNetwork.room.CustomProperties[gameInitKey] == null)
            {
                //Destroy NPCs/Items generated by previous mastet client.
                int GenNPCNum = NPCParent.childCount;
                for (int i = GenNPCNum - 1; i >= 0; i--)
                    PhotonNetwork.Destroy(NPCParent.GetChild(i).gameObject);

                int GenItemNum = itemParent.childCount;
                for (int i = GenItemNum - 1; i >= 0; i--)
                    PhotonNetwork.Destroy(itemParent.GetChild(i).gameObject);

                GameInitByMaster();
            }

        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if (theives.Contains(otherPlayer))
            {
                theives.Remove(otherPlayer);
                thievesNum -= 1;
                UIManager.Instance.RenewThievesNum(thievesNum);
            }
            else
                detectives.Remove(otherPlayer);
            masterPriority.Remove(otherPlayer.ID);
            
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

        private void OnApplicationPause(bool pauseStatus)
        {
            if (!gameStarted)
                return;

            PhotonExtends.SetLocalPlayerPropsByElem(pauseKey, pauseStatus);
            PhotonNetwork.networkingPeer.SendOutgoingCommands();
        }
    }
}
