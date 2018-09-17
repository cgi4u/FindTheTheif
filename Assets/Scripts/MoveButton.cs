﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string direction;
    PlayerController player;

    #region Unity Callbacks

    void Update()
    {
        if (player == null && PlayerController.localPlayer != null)
            player = PlayerController.localPlayer;
    }
    
    #endregion


    #region Methods On Pointer Event

    //Detect current clicks on the Button (the one with the script attached)
    public void OnPointerDown(PointerEventData pointerEventData)
    {
        //Output the name of the GameObject that is being clicked
        //Debug.Log("Go " + direction);
        player.OnMoveButtonPushed(direction);
    }

    //Detect if clicks are no longer registering
    public void OnPointerUp(PointerEventData pointerEventData)
    {
        //Debug.Log("End moving " + direction);
        player.OnMoveButtonReleased(direction);
    }

    #endregion
}
