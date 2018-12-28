using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopUp : MonoBehaviour
{
    public Text attr1;
    public Text attr2;
    public Text attr3;

    public void SetAttributes(string[] attributes)
    {
        if (attributes.Length != 3)
        {
            Debug.LogError("Error: Number of the item's attributes is not 3");
            gameObject.SetActive(false);
            return;
        }

        attr1.text = attributes[0];
        attr2.text = attributes[1];
        attr3.text = attributes[2];
    }
}

