using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class CharTouchEvent : MonoBehaviour
    {
        protected void OnMouseUp()
        {
            if (MultiplayRoomManager.Instance.MyTeam != Team.Detective)
                return;
            
            UIManager.Instance.SetArrestPopUp(gameObject, Input.mousePosition);
        }
    }
}
