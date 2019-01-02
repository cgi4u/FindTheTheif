using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheTheif
{
    public class TouchEvent : MonoBehaviour
    {
        public GameObject touchUI;

        protected void Start()
        {
            touchUI.gameObject.SetActive(false);
        }

        protected void OnMouseDown()
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(transform.position);
            Debug.Log(screenPoint);
            touchUI.gameObject.transform.position = screenPoint;
            touchUI.gameObject.SetActive(true);

            /*
            if (teamOfPlayer == Team.theif)
                Debug.Log("Theif");
            else if (teamOfPlayer == Team.detective)
                Debug.Log("Detective");
            else
                Debug.Log("Error: Undefined");
            */
        }
    }
}
