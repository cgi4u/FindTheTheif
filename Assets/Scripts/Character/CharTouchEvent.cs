using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharTouchEvent : MonoBehaviour
{
    protected void OnMouseUp()
    {
        UIController.Instance.SetCharPopUp(0, transform.position);
    }
}
