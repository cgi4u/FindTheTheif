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

                string direction = "";
                if (horizontal > 0)
                {
                    direction = "right";
                }
                else if (horizontal < 0)
                {
                    direction = "left";
                }
                else if (vertical > 0)
                {
                    direction = "up";
                }
                else if (vertical < 0)
                {
                    direction = "down";
                }

                if (direction != "")
                {
                    Debug.Log("Button pushed stage 1: " + direction);
                    PlayerController.LocalPlayer.OnMoveButtonPushed(direction);
                    PlayerController.LocalPlayer.OnMoveButtonReleased(direction);
                }
            }
        }
    }
}
