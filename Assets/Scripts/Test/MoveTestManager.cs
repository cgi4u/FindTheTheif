using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class MoveTestManager : MonoBehaviour
    {

        void Update()
        {
            float vertical = Input.GetAxis("Vertical");
            float horizontal = Input.GetAxis("Horizontal");

            if (PlayerController.LocalPlayer != null)
            {
                Debug.Log(vertical);
                Debug.Log(horizontal);

                MoveDirection direction = MoveDirection.Stop;
                if (horizontal > 0)
                {
                    direction = MoveDirection.Right;
                }
                else if (horizontal < 0)
                {
                    direction = MoveDirection.Left;
                }
                else if (vertical > 0)
                {
                    direction = MoveDirection.Up;
                }
                else if (vertical < 0)
                {
                    direction = MoveDirection.Down;
                }

                if (direction != MoveDirection.Stop)
                {
                    Debug.Log("Button pushed stage 1: " + direction);
                    PlayerController.LocalPlayer.OnMoveButtonPushed(direction);
                    PlayerController.LocalPlayer.OnMoveButtonReleased(direction);
                }
            }
        }
    }
}
