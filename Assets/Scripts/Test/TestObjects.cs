using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Objects used for test. Should not work in online environmnet.
/// </summary>
public class TestObjects : MonoBehaviour
{
    private void Awake()
    {
        if (PhotonNetwork.connected)
            Destroy(gameObject);
    }
}
