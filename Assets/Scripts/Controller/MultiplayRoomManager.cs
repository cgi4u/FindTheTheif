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

            //Generate NPCs
            NPCGeneration(10);

            //TODO: Generate Items
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
            //조건2. 싱크가 되어야 함.
            if (stream.isWriting)
            {
                stream.SendNext(remainingThief);
                stream.SendNext(timeLeft);
            }
            else
            {
                remainingThief = (int)stream.ReceiveNext();
                timeLeft = (float)stream.ReceiveNext();
            }
        }

        public GameObject NPCPrefab;
        void NPCGeneration(int NPCNum)
        {
            for (int i = 0; i < NPCNum; i++)
            {
                Route randomRoute = RoutingManager.Instance.getRandomRoute();
                Debug.Log("Random route " + i + ": " + randomRoute.gameObject.name);

                GameObject newNPC = PhotonNetwork.InstantiateSceneObject(NPCPrefab.name, new Vector3(0, 0, 0), Quaternion.identity, 0, null);
                newNPC.GetComponent<NPCController>().ManualStart(randomRoute);
            }
        }
    }
}