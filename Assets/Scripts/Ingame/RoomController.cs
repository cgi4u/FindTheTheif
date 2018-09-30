using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : Photon.PunBehaviour, IPunObservable{
    //TODO: 룸 하나에서 전체가 공유해야 하는 데이터와 동작들을 관리
    //조건1. 권한을 주고 받을 수 있어야 함. OnRequest 구현
    //조건2. 싱크가 되어야 함.
    //조건3. 게임이 시작하고 종료될 때 모든 플레이어를 통제할 수 있어야 함. RPC를 통해서 구현 가능할 듯

    //데이터
    // 1. 방 내 현재 남은 도둑의 수
    // 2. 게임 진행 시간

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        throw new System.NotImplementedException();
    }
}
