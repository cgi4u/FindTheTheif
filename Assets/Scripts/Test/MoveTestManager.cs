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

                Direction direction = Direction.None;
                if (horizontal > 0)
                {
                    direction = Direction.Right;
                }
                else if (horizontal < 0)
                {
                    direction = Direction.Left;
                }
                else if (vertical > 0)
                {
                    direction = Direction.Up;
                }
                else if (vertical < 0)
                {
                    direction = Direction.Down;
                }

                if (direction != Direction.None)
                {
                    Debug.Log("Button pushed stage 1: " + direction);
                    PlayerController.LocalPlayer.OnMoveButtonPushed(direction);
                    PlayerController.LocalPlayer.OnMoveButtonReleased(direction);
                }
            }
        }
    }
}
