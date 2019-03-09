﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class CharTouchEvent : MonoBehaviour
    {

        /*
        protected void OnMouseUp()
        {
            if (MultiplayRoomManager.Instance.MyTeam != Team.Detective)
                return;
            
            UIManager.Instance.SetArrestPopUp(gameObject, Input.mousePosition);
        }
        */

        private void Update()
        {
            if (MultiplayRoomManager.Instance.MyTeam != Team.Detective)
                return;

            Sprite curSprite = GetComponent<SpriteRenderer>().sprite;
            Rect touchArea = new Rect(transform.position + curSprite.bounds.min,
                                    curSprite.bounds.max - curSprite.bounds.min);

            if (Application.platform == RuntimePlatform.Android)
            {
                if (Input.touchCount > 0)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        Touch curTouch = Input.GetTouch(i);
                        Vector2 touchedWorldPoint = Camera.main.ScreenToWorldPoint(curTouch.position);
                        if (curTouch.phase == TouchPhase.Ended && touchArea.Contains(touchedWorldPoint))
                            UIManager.Instance.SetArrestPopUp(gameObject, Input.mousePosition);
                    }
                }
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                {
                    Vector2 clickedWorldPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Debug.Log(clickedWorldPoint);
                    if (touchArea.Contains(clickedWorldPoint))
                        UIManager.Instance.SetArrestPopUp(gameObject, Input.mousePosition);
                }
            }
        }
    }
}
