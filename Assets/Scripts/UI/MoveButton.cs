using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace com.MJT.FindTheThief
{
    public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        public Direction direction;

        //Detect current clicks on the Button (the one with the script attached)
        public void OnPointerDown(PointerEventData pointerEventData)
        {
            if (PlayerController.LocalPlayer != null)
                PlayerController.LocalPlayer.OnMoveButtonPushed(direction);
        }

        //Detect if clicks are no longer registering
        public void OnPointerUp(PointerEventData pointerEventData)
        {
            if (PlayerController.LocalPlayer != null)
                PlayerController.LocalPlayer.OnMoveButtonReleased(direction);
        }
    }
}
