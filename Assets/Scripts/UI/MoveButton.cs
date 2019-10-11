using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;

namespace com.MJT.FindTheThief
{
    public interface IMoveButtonDelegate
    {
        void OnMoveButtonPushed(EMoveDirection direction);
        void OnMoveButtonReleased(EMoveDirection direction);
    }

    public class MoveButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        public EMoveDirection direction;

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
