using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCController : Photon.PunBehaviour {
    Vector3 destination;
    public int count = 0;
    
	// Use this for initialization
	void Start () {
        destination = new Vector3(transform.position.x + Random.Range(-1.0f, 1.0f),
                                    transform.position.y + Random.Range(-1.0f, 1.0f));
	}
	
	// Update is called once per frame
	void Update () {
        //transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime);
        if (transform.position == destination)
        {
            destination = new Vector3(transform.position.x + Random.Range(-1.0f, 1.0f),
                                        transform.position.y + Random.Range(-1.0f, 1.0f));
        }
	}

    void OnMouseDown()
    {
        photonView.RequestOwnership();
        if (photonView.isMine)
        {
            count += 1;
        }
        //소유권을 이전받는것과 변수의 값이 싱크되는것과는 별개. 이것도 따로 계속 싱크를 해줘야함
    }

}
