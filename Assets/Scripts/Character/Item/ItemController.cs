﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    string[] attributes
        = new string[] { "Example", "Example", "Example" };

    private void OnMouseUp()
    {
        UIManager.Instance.SetItemPopUp(attributes, transform.position);
    }
}

