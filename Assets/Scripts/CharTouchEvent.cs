using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

public class CharTouchEvent : MonoBehaviour {
    public Team teamOfPlayer;
    public Text testUI;

    private void Start()
    {
        testUI.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
        Debug.Log(screenPoint);
        testUI.gameObject.transform.position = screenPoint;
        testUI.gameObject.SetActive(true);

        if (teamOfPlayer == Team.theif)
            Debug.Log("Theif");
        else if (teamOfPlayer == Team.detective)
            Debug.Log("Detective");
        else
            Debug.Log("Error: Undefined");
    }
}
