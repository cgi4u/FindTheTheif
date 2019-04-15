using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    public class AlertSign : MonoBehaviour
    {
        static readonly float[] quadrantAngles = new float[]
        {
            GlobalFunctions.GetAngle(new Vector2(Screen.width / 2, Screen.height / 2), new Vector2(0f, 0f)),
            GlobalFunctions.GetAngle(new Vector2(-Screen.width / 2, Screen.height / 2), new Vector2(0f, 0f)),
            GlobalFunctions.GetAngle(new Vector2(-Screen.width / 2, -Screen.height / 2), new Vector2(0f, 0f)),
            GlobalFunctions.GetAngle(new Vector2(Screen.width / 2, -Screen.height / 2), new Vector2(0f, 0f))
        };

        public bool isFlicker;

        PlayerController sensingTarget;
        int oldFloorDiff = int.MinValue;

        public Image sameFloorSign;
        public RectTransform upstairGroup;
        public Text upstairText;
        public RectTransform downstairGroup;
        public Text downstairText;

        bool alphaDescending = true;
        float alphaChangeSpeed = 1f;

        public void SetTarget(PlayerController target)
        {
            sensingTarget = target;
        }

        void Update()
        {
            if (sensingTarget == null) return;

            int curFloorDiff = PlayerController.LocalPlayer.CurFloor - sensingTarget.CurFloor;
            if (oldFloorDiff != curFloorDiff)
            {
                oldFloorDiff = curFloorDiff;
                ChangeFloorMode(sensingTarget.CurFloor);
            }
            SetPosAndCycle();

            if (!isFlicker || !sameFloorSign.gameObject.GetActive()) return;

            Color curColor = sameFloorSign.color;
            if (alphaDescending)
            {
                curColor.a -= alphaChangeSpeed * Time.deltaTime;
                if (curColor.a <= 0f)
                {
                    curColor.a = 0f;
                    alphaDescending = false;
                }
            }
            else
            {
                curColor.a += alphaChangeSpeed * Time.deltaTime;
                if (curColor.a >= 1f)
                {
                    curColor.a = 1f;
                    alphaDescending = true;
                }
            }
            sameFloorSign.color = curColor;
        }

        static readonly float maxSensingDist = 40f;

        public void SetPosAndCycle()
        {
            Vector3 diffVec = sensingTarget.transform.position - PlayerController.LocalPlayer.transform.position;

            float dist = diffVec.magnitude;
            dist = Mathf.Min(dist, maxSensingDist);
            float m = (maxSensingDist - dist) / maxSensingDist;
            alphaChangeSpeed = 0.5f + 4.5f * m;
            float signScale = 1.0f + 2.0f * m;

            float angle = GlobalFunctions.GetAngle(diffVec, new Vector2());
            if (angle >= quadrantAngles[2] && angle < quadrantAngles[3])
                m = (-Screen.height / 2) / diffVec.y;
            else if (angle >= quadrantAngles[3] && angle < quadrantAngles[0])
                m = (Screen.width / 2) / diffVec.x;
            else if (angle >= quadrantAngles[0] && angle < quadrantAngles[1])
                m = (Screen.height / 2) / diffVec.y;
            else
                m = (-Screen.width / 2) / diffVec.x;

            sameFloorSign.rectTransform.anchoredPosition = diffVec * m;
            sameFloorSign.rectTransform.localScale = new Vector3(signScale, signScale);
            upstairGroup.anchoredPosition = diffVec * m - new Vector3(0f, upstairGroup.rect.height / 2);
            downstairGroup.anchoredPosition = diffVec * m + new Vector3(0f, downstairGroup.rect.height / 2);
        }

        private void ChangeFloorMode(int floor)
        {
            if (floor < PlayerController.LocalPlayer.CurFloor)
            {
                sameFloorSign.gameObject.SetActive(false);
                upstairGroup.gameObject.SetActive(false);
                downstairGroup.gameObject.SetActive(true);
                downstairText.text = floor.ToString() + "F";
            }
            else if (floor == PlayerController.LocalPlayer.CurFloor)
            {
                sameFloorSign.gameObject.SetActive(true);
                upstairGroup.gameObject.SetActive(false);
                downstairGroup.gameObject.SetActive(false);
            }
            else
            {
                sameFloorSign.gameObject.SetActive(false);
                upstairGroup.gameObject.SetActive(true);
                downstairGroup.gameObject.SetActive(false);
                upstairText.text = floor.ToString() + "F";
            }
        }
    }
}
