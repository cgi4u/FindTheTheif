using System.Collections.Generic;
using ExitGames.Client.Photon;
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

        #region Keys for custom property setting

        static readonly string sceneLoadedKey = "Scene Loaded";
        static readonly string gameInitKey = "Game Initiated";
        static readonly string readyKey = "Ready";
        static readonly string startKey = "Game Started";
        static readonly string gameSetKey = "Game Set";

        private string TeamKey(int id)
        {
            return "Team of Player " + id;
        }
        static readonly string pauseKey = "Pause";

        /*
        static readonly string remainingThievesKey = "Remaining Thieves Number";
        static readonly string remainingTargetsKey = "Remainig Target Items Number";
        */
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
                Debug.LogError("Multiple instantiation of the room Manager.");
                PhotonNetwork.Destroy(this.gameObject);
                return;
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
        /// Select thieves player randomly and load scene.
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

        public GameObject thiefPrefab;
        public GameObject detectivePrefab;

        List<PhotonPlayer> detectivePlayers = new List<PhotonPlayer>();
        List<PhotonPlayer> thiefPlayers = new List<PhotonPlayer>();
        List<int> masterPriority = new List<int>();
        private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene != SceneManager.GetSceneByName(roomLevelName))
            {
                Debug.LogError("A wrong scene is loaded. Scene name: " + SceneManager.GetActiveScene().name);
                return;
            }

            mapDataManager = MapDataManager.Instance;

            //Initilaize team information and instantiate the local player.
            myTeam = (Team)PhotonNetwork.room.CustomProperties[TeamKey(PhotonNetwork.player.ID)];
            UIManager.Instance.SetTeamLabel(myTeam);
            UIManager.Instance.RenewThievesNum(thievesNum);

            foreach (PhotonPlayer player in PhotonNetwork.playerList)
            {
                Team teamOfPlayer = (Team)PhotonNetwork.room.CustomProperties[TeamKey(player.ID)];
                if (teamOfPlayer == Team.Detective)
                {
                    detectivePlayers.Add(player);
                }
                else if (teamOfPlayer == Team.Thief)
                {
                    thiefPlayers.Add(player);
                }
            }

            Debug.Log("We are Instantiating LocalPlayer from " + SceneManager.GetActiveScene().name);
            // we're in a room. spawn a character for the local player. it gets synced by using PhotonNetwork.Instantiate
            if (myTeam == Team.Detective)
                PhotonNetwork.Instantiate(detectivePrefab.name, new Vector3(0f + PhotonNetwork.player.ID, -5f, 0f), Quaternion.identity, 0);
            else if (myTeam == Team.Thief)
                PhotonNetwork.Instantiate(thiefPrefab.name, new Vector3(0f + PhotonNetwork.player.ID, -5f, 0f), Quaternion.identity, 0);

            // Set master client priority  
            foreach (PhotonPlayer thiefPlayer in thiefPlayers)
                masterPriority.Add(thiefPlayer.ID);
            masterPriority.Sort();

            List<int> detectivePriority = new List<int>();
            foreach (PhotonPlayer detectivePlayer in thiefPlayers)
                detectivePriority.Add(detectivePlayer.ID);
            detectivePriority.Sort();

            masterPriority.AddRange(detectivePriority);

            // Other initialization for this game.
            ItemController.ResetDescoverdItems();

            // Game initiation by the master client.
            if (PhotonNetwork.isMasterClient)
            {
                GameInitByMaster();
            }

            PhotonExtends.SetLocalPlayerPropsByElem(readyKey, true);
            inReady = true;
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

            GenerateItems();

            // Game initiation ends, send ready signal to other players. 
            PhotonExtends.SetRoomCustomPropsByElem(gameInitKey, true);
        }

        public int numberOfNPC;
        public GameObject NPCPrefab;
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
                //issue: Error
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
                                        mapDataManager.ItemGenPoints[i].transform.position, Quaternion.identity, 0, null);
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
                photonView.RPC("ActivatePutItemPointInRoom", PhotonTargets.All, roomsInFloor[floor][r]);
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

            if ((Team)PhotonNetwork.room.CustomProperties[TeamKey(PhotonNetwork.player.ID)] == Team.Thief)
                UIManager.Instance.RenewTargetItemList(targetItems);
        }

        [PunRPC]
        private void ActivatePutItemPointInRoom(int roomIndex)
        {
            mapDataManager.Rooms[roomIndex].PutItemPoint.Activated = true;
        }



        #endregion

        #region Shared Ingame Activities

        // Start/End time of the game(use server time to set)
        [SerializeField]
        private int leftTimeStamp;
        [SerializeField]
        bool inReady = false;
        [SerializeField]
        bool gameStarted = false;
        [SerializeField]
        bool gameSet = false;

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

            if (inReady && PhotonNetwork.isMasterClient && PhotonNetwork.room.CustomProperties[gameInitKey] != null)
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
                }
            }

            if (!gameStarted)
                return;
            // Section after game is started.

            leftTimeStamp -= deltaTimeStamp;
            UIManager.Instance.RenewTimeLabel(leftTimeStamp / 1000);
        }

        private void LateUpdate()
        {
            if (!gameStarted)
                return;
            // Section after game is started

            // If the local player is the master and in detective team, change the master client to be a thief if it is possible.
            if (PhotonNetwork.isMasterClient && myTeam == Team.Detective)
            {
                foreach (PhotonPlayer thiefPlayer in thiefPlayers)
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
        /// Takeover master client from other client. Used to change the master client from detective to thief player.
        /// </summary>
        /// <param name="sentTimestamp">Timestamp when RPC was fired. If timestamp interval is too long, it is considered as timeout(default = 5000ms).</param>
        [PunRPC]
        private void TakeOverMaster(int sentTimestamp)
        {
            if (!PhotonNetwork.isMasterClient && PhotonNetwork.ServerTimestamp - sentTimestamp < 5000)
                PhotonNetwork.SetMasterClient(PhotonNetwork.player);
        }

        #endregion

        #region Detective Gameplay Networking(Arrest Thief)

        int arrestedThievesNum = 0;

        [PunRPC]
        public void ArrestThief(int thiefID)
        {
            if (PhotonPlayer.Find(thiefID) == null)
            {
                Debug.LogError("Arrest failed. Wrong Thief ID.");
                return;
            }

            UIManager.Instance.RenewErrorLabel("Thief " + thiefID + " is arrested.");
            arrestedThievesNum += 1;
            UIManager.Instance.RenewThievesNum(thievesNum - arrestedThievesNum);

            if (thievesNum == arrestedThievesNum)
            {
                PhotonExtends.SetRoomCustomPropsByElem(gameSetKey, true);
            }

            if (PhotonNetwork.player.ID == thiefID)
            {
                UIManager.Instance.SetObserverModeUI();
            }
        } 

        [PunRPC]
        public void ArrestFailed(int detectiveID)
        {
            UIManager.Instance.RenewErrorLabel("Detective " + PhotonPlayer.Find(detectiveID).NickName + " failed to arrest.");
        }

        #endregion

        #region Thief Gameplay Networking(Steal Item)

        /// <summary>
        /// The list of items stolen(not just carried by thief, but put into the destination point.)
        /// </summary>
        private List<ItemController> stolenItems = new List<ItemController>();
        [PunRPC]
        public void ItemStolen(int pointOfStolenItem)
        {
            ItemController stolenItem = mapDataManager.ItemGenPoints[pointOfStolenItem].Item;
            stolenItems.Add(stolenItem);
            ItemController.RenewDiscoveredItemPrioirty(stolenItem);
            UIManager.Instance.RenewStolenItemList(stolenItems);

            UIManager.Instance.RenewErrorLabel("Item " + stolenItem.name + " is stolen.");

            if (targetItems.Contains(stolenItem))
            {
                targetItems.Remove(stolenItem);
                if (myTeam == Team.Thief)
                    UIManager.Instance.RenewTargetItemList(targetItems);

                if (targetItems.Count == 0 && PhotonNetwork.isMasterClient)
                {
                    PhotonExtends.SetRoomCustomPropsByElem(gameSetKey, true);
                }
            } 
        }

        #endregion

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
                int sceneObjNum = sceneObjParent.childCount;
                for (int i = sceneObjNum - 1; i >= 0; i--)
                    PhotonNetwork.Destroy(sceneObjParent.GetChild(i).gameObject);

                GameInitByMaster();
            }

        }

        public override void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
        {
            if (thiefPlayers.Contains(otherPlayer))
            {
                thiefPlayers.Remove(otherPlayer);
                thievesNum -= 1;
                UIManager.Instance.RenewThievesNum(thievesNum - arrestedThievesNum);
            }
            else
                detectivePlayers.Remove(otherPlayer);
            masterPriority.Remove(otherPlayer.ID);
            
            CheckIfGameSet();
        }

        public override void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
        {
            if (propertiesThatChanged[startKey] != null 
                && (bool)propertiesThatChanged[startKey] && !gameStarted)
            {
                inReady = false;
                gameStarted = true;
                return;
            }

            if (propertiesThatChanged[gameSetKey] != null
                    && (bool)propertiesThatChanged[gameSetKey] == true && !gameSet)
            {
                Time.timeScale = 0;
                UIManager.Instance.RenewErrorLabel("Game Set");

                gameSet = true;
                gameStarted = false;
                return;
            }
        }

        public void CheckIfGameSet()
        {
            if (thievesNum - arrestedThievesNum == 0)
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