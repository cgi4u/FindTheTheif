using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace com.MJT.FindTheThief
{
    public class MultiplayRoomManager : Photon.PunBehaviour
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
        UIManager uiManager;

        #region Keys for custom property setting

        readonly string teamSettingEndKey = "Player Team Set";
        readonly string initializedKey = "Game Initiated";
        readonly string playerReadyKey = "Player Ready";
        readonly string startTimeKey = "Game Start Timestamp";
        readonly string gameSetKey = "Game Set";
        readonly string winTeamKey = "Win Team";

        readonly string TeamKey = "My team";
        readonly string playerGenPointKey = "Player Generation Point";
        readonly string theifFigureIdxKey = "Theif Figure";

        readonly string playerRecordsKey = "Player Records";
        readonly string playerRecordsNumKey = "Player Records Number";

        readonly string pauseKey = "Paused";

        /*
        static readonly string remainingThievesKey = "Remaining Thieves Number";
        static readonly string remainingTargetsKey = "Remainig Target Items Number";
        */

        #endregion

        /// <summary>
        /// Local client's current game state.
        /// </summary>
        public EGameState GameState { get; set; } = EGameState.None;
        private int thievesNum;

        private void Awake()
        {
            if (!PhotonNetwork.connected)
            {
                Debug.LogError("Multiplay manager must be used in online environment.");
                return;
            }

            // Modify PhotonNetwork settings according to in-game mode.
            PhotonNetwork.BackgroundTimeout = 10f;
            PhotonNetwork.sendRate = 10;
            PhotonNetwork.sendRateOnSerialize = 10;

            //Set the singleton
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.LogError("Multiple instantiation of the room Manager.");
                PhotonNetwork.Destroy(this.gameObject);
                return;
            }

            mapDataManager = MapDataManager.Instance;
            uiManager = UIManager.Instance;

            Hashtable roomCp = PhotonNetwork.room.CustomProperties;

            //Get the number of NPCs(only in test version).
            int tempNPCNum;
            if (int.TryParse(roomCp[Constants.NPCNumKey].ToString(), out tempNPCNum))
            {
                NPCNum = tempNPCNum;
            }

            //Get the number of thief players.
            if (!int.TryParse(roomCp["Thieves Number"].ToString(), out thievesNum))
            {
                Debug.LogError("Thieves number(in custom property) is not set properly.");
                return;
            }

            PhotonExtends.SetLocalPlayerProp(pauseKey, false);

            if (PhotonNetwork.isMasterClient) {
                //Randomly switch the master client. It prevents that the player who made room always be picked as a thief.
                int[] randomPlayerSelector = new int[PhotonNetwork.playerList.Length];
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                    randomPlayerSelector[i] = PhotonNetwork.playerList[i].ID;

                Globals.RandomizeArray<int>(randomPlayerSelector);

                if (randomPlayerSelector[0] == PhotonNetwork.player.ID)
                    SetTeamOfPlayers();
                else
                {
                    Debug.Log("Change the master client.");

                    for (int i = 0; i < randomPlayerSelector.Length; i++)
                    {
                        PhotonPlayer newMaster = PhotonPlayer.Find(randomPlayerSelector[i]);
                        if (newMaster != null 
                            && newMaster.CustomProperties[pauseKey] != null && !(bool)newMaster.CustomProperties[pauseKey])
                        {
                            PhotonNetwork.SetMasterClient(newMaster);
                            break;
                        }
                    }
                }
            }
        }

        private ETeam myTeam;
        /// <summary>
        /// Team of the local player.
        /// </summary>
        public ETeam MyTeam
        {
            get
            {
                return myTeam;
            }
        }

        List<PhotonPlayer> detectivePlayers;
        List<PhotonPlayer> thiefPlayers;
        List<int> masterPriority;

        /// <summary>
        /// Select thieves player randomly and load scene.
        /// Should be called only by the master client. The master client must not be a detective because of sync delay issue.
        /// Should be called when reset entire game.
        /// </summary>
        private void SetTeamOfPlayers()
        {
            if (!PhotonNetwork.isMasterClient)
            {
                Debug.LogError("Team setting must be operated by the master client.");
                return;
            }

            int playerNum = PhotonNetwork.playerList.Length;
            if (playerNum <= thievesNum)
            {
                Debug.LogError("Not enough player for game(Should be more than " + thievesNum + " players(number of thieves).");
                return;
            }

            
            // Set each player's team.
            // Default value is detective, and thief players is selected by randomized array.
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                Hashtable playerCp = player.CustomProperties;
                playerCp[TeamKey] = (int)ETeam.Detective;
                player.SetCustomProperties(playerCp);
            }

            // Choose thief players randomly
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
            PhotonExtends.SetLocalPlayerProp(TeamKey, (int)ETeam.Thief);
            for (int i = 0; i < thievesNum - 1; i++)
            {
                PhotonPlayer thiefPlayer = PhotonPlayer.Find(thiefSelector[i]);
                Hashtable playerCp = thiefPlayer.CustomProperties;
                playerCp[TeamKey] = (int)ETeam.Detective;
                thiefPlayer.SetCustomProperties(playerCp);
            }

            PhotonExtends.SetRoomCustomProp(teamSettingEndKey, true);
        }

        private void InitializeGame()
        {
            //Initilaize team information and instantiate the local player.
            myTeam = (ETeam)PhotonNetwork.player.CustomProperties[TeamKey];

            //Set UI Informations and deactive the interactive UI before game start).
            uiManager.SetTeamLabel(myTeam);
            uiManager.RenewThievesNum(thievesNum);
            uiManager.DeactivateUIGroup();

            detectivePlayers = new List<PhotonPlayer>();
            thiefPlayers = new List<PhotonPlayer>();
            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                ETeam teamOfPlayer = (ETeam)player.CustomProperties[TeamKey];
                if (teamOfPlayer == ETeam.Detective)
                {
                    detectivePlayers.Add(player);
                }
                else if (teamOfPlayer == ETeam.Thief)
                {
                    thiefPlayers.Add(player);
                }
            }

            // Set master client priority 
            masterPriority = new List<int>();
            foreach (PhotonPlayer thiefPlayer in thiefPlayers)
                masterPriority.Add(thiefPlayer.ID);
            masterPriority.Sort();

            List<int> detectivePriority = new List<int>();
            foreach (PhotonPlayer detectivePlayer in thiefPlayers)
                detectivePriority.Add(detectivePlayer.ID);
            detectivePriority.Sort();

            masterPriority.AddRange(detectivePriority);

            // Initilize discoverd item list.
            ItemController.ResetDescoverdItems();
            PutItemPoint.ResetActivatedPointList();

            // Game initiation by the master client.
            if (PhotonNetwork.isMasterClient)
            {
                // Set generation points of players and generate scene objects.
                SetGenPointAndGenSceneObj();
            }
        }

        [SerializeField]
        private Transform sceneObjParent;
        /// <summary>
        /// The parent of all scene objects(NPCs, Items) in Unity object hierarchy.
        /// </summary>
        public Transform SceneObjParent
        {
            get
            {
                return sceneObjParent;
            }
        }

        private List<int> randomNPCSelector = new List<int>();

        /// <summary>
        /// Generate NPCs and Items for current game. Should be called by the master client.
        /// </summary>
        private void SetGenPointAndGenSceneObj()
        {
            if (!PhotonNetwork.isMasterClient) {
                Debug.LogError("Object generation must be operated by the master client.");
                return;
            }

            // Set the starting point of each players and generate the local players.
            int[] thiefGenPointSelector = new int[mapDataManager.ThiefGenertaionPoints.Count];
            for (int i = 0; i < thiefGenPointSelector.Length; i++)
                thiefGenPointSelector[i] = i;
            Globals.RandomizeArray<int>(thiefGenPointSelector);

            for (int i = 0; i < NPCPrefabs.Count; i++)
                randomNPCSelector.Add(i);
            Globals.RandomizeList<int>(randomNPCSelector);

            //Set the avatar of the thief player from NPC list and remove selected NPCs from the list.
            //And set the generation points of the thief players.
            for (int i = 0; i < thiefPlayers.Count; i++)
            {
                Hashtable cp = thiefPlayers[i].CustomProperties;
                cp[playerGenPointKey] = thiefGenPointSelector[i];
                cp[theifFigureIdxKey] = randomNPCSelector[0];
                thiefPlayers[i].SetCustomProperties(cp);

                randomNPCSelector.RemoveAt(0);
            }

            //Set the generation points of the detective players.
            for (int i = 0; i < detectivePlayers.Count; i++)
            {
                Hashtable cp = detectivePlayers[i].CustomProperties;
                cp[playerGenPointKey] = i;
                detectivePlayers[i].SetCustomProperties(cp);
            }

            // Generate Scene Objects(NPCs, Items).
            GenerateNPC();
            GenerateItems();

            // Game initiation ends, send ready signal to other players. 
            PhotonExtends.SetRoomCustomProp(initializedKey, true);
        }

        public int NPCNum;
        public NPCPrefabSet NPCPrefabs;

        /// <summary>
        /// Generate NPCs at random nodes.
        /// </summary>
        private void GenerateNPC()
        {
            if (NPCNum > randomNPCSelector.Count)
            {
                Debug.LogError("Error: Attempt to generate more number of NPCs than available NPC prefabs.");
                return;
            }

            for (int i = 0; i < NPCNum; i++)
            {
                int randomPoint = mapDataManager.GetRandomNPCGenPoint();
                if (randomPoint == -1)
                {
                    Debug.LogError("Error: Attempt to generate more number of NPCs than available generation points.");
                    return;
                }

                GameObject newNPC = PhotonNetwork.InstantiateSceneObject("NPCs\\" + NPCPrefabs.Get(randomNPCSelector[i]).name, 
                                                                            new Vector3(0, 0, 0), Quaternion.identity, 0, null);
                PhotonView.Get(newNPC).RPC("Init", PhotonTargets.All, randomPoint);
            }
        }

        public GameObject[] itemPrefabs;
        public int targetItemNum; 
        [SerializeField]
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
            if (numOfItemGenPoint < targetItemNum)
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
                    Debug.Log("Selected property: " + (EItemColor)selectedProp);
                    break;
                case 1:
                    Debug.Log("Selected property: " + (EItemAge)selectedProp);
                    break;
                case 2:
                    Debug.Log("Selected property: " + (EItemUsage)selectedProp);
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
                        if (itemController.Color == (EItemColor)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                    case 1:
                        if (itemController.Age == (EItemAge)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                    case 2:
                        if (itemController.Usage == (EItemUsage)selectedProp)
                            ItemsHaveProp.Add(item);
                        break;
                }
            }


            // Select steal target/non-target items to generate; 
            Globals.RandomizeList<GameObject>(ItemsHaveProp);
            List<GameObject> targetItems = new List<GameObject>();
            for (int i = 0; i < targetItemNum; i++)
                targetItems.Add(ItemsHaveProp[i]);

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
                // Not allow the case in which multiple items in a same room.
                ExhibitRoom roomOfPoint = mapDataManager.ItemGenPoints[targetItemPointSelector[i]].GetComponentInParent<ExhibitRoom>();
                if (!roomContainTargetItem.Contains(roomOfPoint))
                {
                    targetItemPoints.Add(targetItemPointSelector[i]);
                    roomContainTargetItem.Add(roomOfPoint);
                    if (targetItemPoints.Count == targetItemNum)
                        break;
                }
            }
            if (targetItemPoints.Count != targetItemNum)
            {
                Debug.LogError("There are not enough rooms for item generation.");
                return;
            }

            // Generate items.
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
                                        mapDataManager.ItemGenPoints[i].ItemPos, Quaternion.identity, 0, null);
                PhotonView.Get(newItem).RPC("Init", PhotonTargets.All, i);
            }

            photonView.RPC("SetTargetItemList", PhotonTargets.All, targetItemPoints.ToArray());

            // Select put item point to activate in this game.
            Dictionary<int, List<int>> roomsInFloor = new Dictionary<int, List<int>>();

            for (int i = 0; i < mapDataManager.Rooms.Count; i++)
            {
                ExhibitRoom room = mapDataManager.Rooms[i];

                if (!roomsInFloor.ContainsKey(room.Floor))
                    roomsInFloor[room.Floor] = new List<int>();
                roomsInFloor[room.Floor].Add(i);
            }

            foreach (int floor in roomsInFloor.Keys)
            {
                int r = Random.Range(0, roomsInFloor[floor].Count);
                int randRoomIdx = roomsInFloor[floor][r];
                int randPutPointIdx = Random.Range(0, mapDataManager.Rooms[randRoomIdx].PutItemPoints.Length);
                //issue: 인덱스 오류남
                photonView.RPC("ActivatePutItemPointInRoom", PhotonTargets.All, randRoomIdx, randPutPointIdx);
            }
        }

        /// <summary>
        /// Set target items which the player should be steal(For a thief player).
        /// </summary>
        /// <param name="targetItemPoints">Indices of item generation points which have target items.</param>
        [PunRPC]
        private void SetTargetItemList(int[] targetItemPoints)
        {
            for (int i = 0; i < targetItemPoints.Length; i++)
                targetItems.Add(mapDataManager.ItemGenPoints[targetItemPoints[i]].Item);

            if ((ETeam)PhotonNetwork.player.CustomProperties[TeamKey] == ETeam.Thief)
                uiManager.RenewTargetItemList(targetItems);
        }

        /// <summary>
        /// Activate randomly picked item put point for thief players.
        /// </summary>
        /// <param name="roomIdx">Randomly picked room index.</param>
        /// <param name="pointIdx">Randomly picked item put point index.</param>
        [PunRPC]
        private void ActivatePutItemPointInRoom(int roomIdx, int pointIdx)
        {
            mapDataManager.Rooms[roomIdx].PutItemPoints[pointIdx].Activate();
        }

        private void GenLocalPlayerAndReady()
        {
            int genPointIdx = (int)PhotonNetwork.player.CustomProperties[playerGenPointKey];
            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            Debug.Log("You are a " + myTeam);
            if (myTeam == ETeam.Detective)
                PhotonNetwork.Instantiate(detectivePrefab.name, mapDataManager.DetectiveGenerationPoints[genPointIdx].position, Quaternion.identity, 0);
            else if (myTeam == ETeam.Thief)
            {
                GameObject myTheif = PhotonNetwork.Instantiate(thiefPrefab.name, mapDataManager.ThiefGenertaionPoints[genPointIdx].position, Quaternion.identity, 0);

                int NPCIdx = (int)PhotonNetwork.player.CustomProperties[theifFigureIdxKey];
                PhotonView.Get(myTheif).RPC("SetSpriteAndAnimation", PhotonTargets.All, NPCIdx);
            }
            GameState = EGameState.Ready;

            if (PhotonNetwork.isMasterClient)
            {
                foreach (NPCController npc in sceneObjParent.GetComponentsInChildren<NPCController>())
                {
                    npc.Activated = true;
                }

                PhotonExtends.SetRoomCustomProp(startTimeKey, PhotonNetwork.ServerTimestamp + 3000);
            }
        }

        public bool PrepareTimeEnd { get; set; } = false;

        // Start/End time of the game(use server time to set)
        [SerializeField]
        private int timestampForPrepare;
        private int prepareStartTimestamp;

        [SerializeField]
        private int timestampPerGame;
        private int startTimestamp;

        int curTimestamp;
        int prevTimestamp;
        private void Start()
        {
            prevTimestamp = PhotonNetwork.ServerTimestamp;
        }

        int readyWaitTimeStamp = 0;
        private void Update()
        {
            //Debug.Log(PhotonNetwork.networkingPeer.RoundTripTime);

            curTimestamp = PhotonNetwork.ServerTimestamp;
            int deltaTimeStamp = curTimestamp - prevTimestamp;
            prevTimestamp = curTimestamp;

            // Run after player became to ready state.
            switch (GameState)
            {
                case EGameState.ReadyChecked:
                    if (curTimestamp - prepareStartTimestamp < 0)
                        break;

                    // Check if all players in ready state.
                    bool allPlayersReady = true;
                    foreach (PhotonPlayer player in PhotonNetwork.playerList)
                    {
                        object readyState = player.CustomProperties[playerReadyKey];
                        if (readyState == null)
                        {
                            allPlayersReady = false;
                            break;
                        }
                    }

                    // If there is a player which is not in ready, postpone the start time and resend the time to all players
                    if (!allPlayersReady)
                    {
                        if (PhotonNetwork.isMasterClient)
                            photonView.RPC("PostponeStartTime", PhotonTargets.All, curTimestamp + 5000);
                    }
                    else
                    {
                        if (myTeam == ETeam.Thief)
                            uiManager.ActivateUIGroup();
                        GameState = EGameState.Preparing;
                    }

                    break;
                case EGameState.Preparing:
                    // If preparing time is not over, renew time jabel.
                    if (timestampForPrepare - (curTimestamp - prepareStartTimestamp) > 0) {
                        uiManager.RenewTimeLabel((timestampForPrepare - (curTimestamp - prepareStartTimestamp)) / 1000);
                    }
                    // If prepareing time ends, let the game start.
                    else
                    {
                        if (myTeam == ETeam.Detective)
                            uiManager.ActivateUIGroup();
                        startTimestamp = prepareStartTimestamp + timestampForPrepare;
                        GameState = EGameState.Started;
                    }

                    break;
                case EGameState.Started:
                    // Renew time label, view current remaining time.
                    uiManager.RenewTimeLabel((timestampPerGame - (curTimestamp - startTimestamp)) / 1000);
                    break;
                default:
                    break;
            }
        }

        [PunRPC]
        private void PostponeStartTime(int newPrepareStartTimestamp)
        {
            int prepareStartTimestamp = newPrepareStartTimestamp;
            if (GameState == EGameState.Ready)
            {
                GameState = EGameState.ReadyChecked;
                PhotonExtends.SetLocalPlayerProp(playerReadyKey, true);
            }
        }

        private void LateUpdate()
        {
            if (GameState != EGameState.Preparing && GameState != EGameState.Started)
                return;
            // Run only after game is preparing or started state.

            // If the local player is the master and in detective team, change the master client to be a thief if it is possible.
            if (PhotonNetwork.isMasterClient && myTeam == ETeam.Detective)
            {
                foreach (PhotonPlayer thiefPlayer in thiefPlayers)
                {
                    if ((bool)thiefPlayer.CustomProperties[pauseKey] == false)
                    {
                        PhotonNetwork.networkingPeer.SendOutgoingCommands();
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
        /// Takeover master client from other client. Used to change the master client from detective to thief player.
        /// </summary>
        /// <param name="sentTimestamp">Timestamp when RPC was fired. If timestamp interval is too long, it is considered as timeout(default = 5000ms).</param>
        [PunRPC]
        private void TakeOverMaster(int sentTimestamp)
        {
            if (!PhotonNetwork.isMasterClient && PhotonNetwork.ServerTimestamp - sentTimestamp < 5000)
                PhotonNetwork.SetMasterClient(PhotonNetwork.player);
        }

        private void SetPlayerRecord(int newRecord)
        {
            Hashtable cp = PhotonNetwork.player.CustomProperties;
            if (cp[playerRecordsNumKey] == null)
            {
                cp[playerRecordsKey + 0] = newRecord;
                cp[playerRecordsNumKey] = 1;
            }
            else
            {
                int recordNum = (int)cp[playerRecordsNumKey];
                
                cp[playerRecordsKey + recordNum] = newRecord;
                cp[playerRecordsNumKey] = recordNum + 1;
            }
            PhotonNetwork.player.SetCustomProperties(cp);
        }

        #region Detective Gameplay Networking(Arrest Thief)

        int arrestedThievesNum = 0;

        [PunRPC]
        public void ArrestSuccess(int detectiveID, int thiefID)
        {
            if (PhotonPlayer.Find(thiefID) == null)
            {
                Debug.LogError("Arrest failed. Wrong Thief ID.");
                return;
            }

            uiManager.SetErrorMsg("Thief " + thiefID + " is arrested.");
            arrestedThievesNum += 1;
            uiManager.RenewThievesNum(thievesNum - arrestedThievesNum);

            if (detectiveID == PhotonNetwork.player.ID)
            {
                SetPlayerRecord(curTimestamp - startTimestamp);
            }

            if (PhotonNetwork.player.ID == thiefID)
            {
                uiManager.SetObserverModeUI();
            }

            if (thievesNum == arrestedThievesNum)
            {
                PhotonExtends.SetRoomCustomProp(winTeamKey, (int)ETeam.Detective);
                PhotonExtends.SetRoomCustomProp(gameSetKey, true);
            }
        } 

        [PunRPC]
        public void ArrestFailed(int detectiveID)
        {
            uiManager.SetErrorMsg("Detective " + PhotonPlayer.Find(detectiveID).NickName + " failed to arrest.");
        }

        #endregion

        #region Thief Gameplay Networking(Steal Item)

        /// <summary>
        /// The list of items stolen(not just carried by thief, but put into the destination point.)
        /// </summary>
        private List<ItemController> stolenItems = new List<ItemController>();
        [PunRPC]
        public void StealSuccess(int thiefID, int pointOfStolenItem)
        {
            ItemController stolenItem = mapDataManager.ItemGenPoints[pointOfStolenItem].Item;
            stolenItems.Add(stolenItem);
            ItemController.RenewDiscoveredItemPrioirty(stolenItem);
            uiManager.RenewStolenItemList(stolenItems);

            uiManager.SetErrorMsg("Item " + stolenItem.name + " is stolen.");

            if (targetItems.Contains(stolenItem))
            {
                targetItems.Remove(stolenItem);
                if (myTeam == ETeam.Thief)
                    uiManager.RenewTargetItemList(targetItems);

                if (thiefID == PhotonNetwork.player.ID)
                {
                    SetPlayerRecord(curTimestamp - startTimestamp);
                }

                if (targetItems.Count == 0 && PhotonNetwork.isMasterClient)
                {
                    PhotonExtends.SetRoomCustomProp(winTeamKey, (int)ETeam.Thief);
                    PhotonExtends.SetRoomCustomProp(gameSetKey, true);
                }
            } 
        }

        #endregion

        public override void OnMasterClientSwitched(PhotonPlayer newMasterClient)
        {
            Debug.Log("Master client switched. New Master ID: " + newMasterClient.ID);
            if (newMasterClient != PhotonNetwork.player)
                return;

            Debug.Log("You were assgined as the new master client.");
            if (PhotonNetwork.room.CustomProperties[teamSettingEndKey] == null)
                SetTeamOfPlayers();
            else if (PhotonNetwork.room.CustomProperties[initializedKey] == null)
            {
                //Destroy NPCs/Items generated by previous mastet client.
                int sceneObjNum = sceneObjParent.childCount;
                for (int i = sceneObjNum - 1; i >= 0; i--)
                    PhotonNetwork.Destroy(sceneObjParent.GetChild(i).gameObject);

                SetGenPointAndGenSceneObj();
            }

        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if (GameState == EGameState.Game_Set)
                return;

            if (GameState != EGameState.Started && GameState != EGameState.Preparing)
            {
                LeaveRoomWithError("유저가 게임 준비 도중 접속을 해제했습니다");
                return;
            }


            // Remove player from player list and the master client priorty list.
            if (thiefPlayers.Contains(otherPlayer))
            {
                thiefPlayers.Remove(otherPlayer);
                thievesNum -= 1;
                uiManager.RenewThievesNum(thievesNum - arrestedThievesNum);
            }
            else
                detectivePlayers.Remove(otherPlayer);
            masterPriority.Remove(otherPlayer.ID);

            // If one team has no member, the oppenent team is win.
            if (thievesNum - arrestedThievesNum == 0)
            {
                PhotonExtends.SetRoomCustomProp(winTeamKey, (int)ETeam.Detective);
                PhotonExtends.SetRoomCustomProp(gameSetKey, true);
            }
            else if (PhotonNetwork.playerList.Length == thievesNum)
            {
                PhotonExtends.SetRoomCustomProp(winTeamKey, (int)ETeam.Thief);
                PhotonExtends.SetRoomCustomProp(gameSetKey, true);
            }
        }

        public string lobbySceneName;
        public string loginSceneName;
        /// <summary>
        /// Leave room when there is an unexpected error.
        /// </summary>
        /// <param name="errorMsg">Error message.</param>
        private void LeaveRoomWithError(string errorMsg)
        {
            Debug.Log(errorMsg);

            if (PhotonNetwork.connected)
            {
                PhotonNetwork.LeaveRoom();
                SceneManager.LoadScene(lobbySceneName);
            }
            else
                SceneManager.LoadScene(loginSceneName);
        }

        public GameObject thiefPrefab;
        public GameObject detectivePrefab;

        public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
        {
            if (GameState == EGameState.None && propertiesThatChanged[teamSettingEndKey] != null)
            {
                GameState = EGameState.Team_Set;
                InitializeGame();
                return;
            }

            if (GameState == EGameState.Team_Set && propertiesThatChanged[initializedKey] != null)
            {
                GameState = EGameState.Initialized;
                GenLocalPlayerAndReady();
                return;
            }

            if (GameState == EGameState.Ready && propertiesThatChanged[startTimeKey] != null)
            {
                prepareStartTimestamp = (int)propertiesThatChanged[startTimeKey];

                if (PhotonNetwork.ServerTimestamp - prepareStartTimestamp < 0)
                {
                    GameState = EGameState.ReadyChecked;
                    PhotonExtends.SetLocalPlayerProp(playerReadyKey, true);
                }
            }

            //When Game set key is set by the master client, let the game be end.
            if (propertiesThatChanged[gameSetKey] != null)
            {
                PhotonNetwork.automaticallySyncScene = false;
                GameState = EGameState.Game_Set;

                ETeam winTeam = (ETeam)propertiesThatChanged[winTeamKey];
                if (winTeam == myTeam)
                    uiManager.SetWinPanel();
                else
                    uiManager.SetLosePanel();

                StartCoroutine("LoadRecordScene");

                foreach (PhotonPlayer player in PhotonNetwork.playerList)
                {
                    Debug.Log("Player " + player.NickName + "\'s Record");
                    if (player.CustomProperties[playerRecordsNumKey] != null)
                    {
                        int recordsNum = (int)player.CustomProperties[playerRecordsNumKey];
                        for (int i = 0; i < recordsNum; i++)
                        {
                            int record = (int)player.CustomProperties[playerRecordsKey + i];
                            Debug.Log(record);
                        }
                    }
                }
            }
        }

        public string recordSceneName;

        IEnumerator LoadRecordScene()
        {
            yield return new WaitForSeconds(1f);

            //Destroy scene objects in local client(since game is ended).
            for (int i = sceneObjParent.childCount - 1; i >= 0; i--)
                Destroy(sceneObjParent.GetChild(i).gameObject);
            SceneManager.LoadScene(recordSceneName);
        }

        public override void OnLeftRoom()
        {
            instance = null;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (GameState != EGameState.Preparing && GameState != EGameState.Started)
                return;

            PhotonExtends.SetLocalPlayerProp(pauseKey, pauseStatus);
            PhotonNetwork.networkingPeer.SendOutgoingCommands();
        }
    }
}


#region Test Codes

/* 
 if (PhotonNetwork.playerList.Length != 1)
            {
                for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
                {
                    if (PhotonNetwork.playerList[i].ID != PhotonNetwork.player.ID
                        && PhotonNetwork.room.CustomProperties["Changed"] == null)
                    {
                        PhotonNetwork.SetMasterClient(PhotonNetwork.playerList[i]);
                        PhotonExtends.SetRoomCustomPropsByElem("Changed", true);
                        return;
                    }
                }
            }
*/
#endregion