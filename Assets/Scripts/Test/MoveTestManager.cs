using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
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

                MoveDirection direction = MoveDirection.stop;
                if (horizontal > 0)
                {
                    direction = MoveDirection.right;
                }
                else if (horizontal < 0)
                {
                    direction = MoveDirection.left;
                }
                else if (vertical > 0)
                {
                    direction = MoveDirection.up;
                }
                else if (vertical < 0)
                {
                    direction = MoveDirection.down;
                }

                if (direction != MoveDirection.stop)
                {
                    Debug.Log("Button pushed stage 1: " + direction);
                    PlayerController.LocalPlayer.OnMoveButtonPushed(direction);
                    PlayerController.LocalPlayer.OnMoveButtonReleased(direction);
                }
            }
        }
    }
}
