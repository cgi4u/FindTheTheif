using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIController : MonoBehaviour
{
    public static UIController uIController;

    void Start()
    {
        if (uIController == null)
            uIController = this;

        charPopUp.SetActive(false);
        itemPopUp.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (charPopUp.GetActive())
            {
                charPopUp.SetActive(false);
            }
            if (itemPopUp.gameObject.GetActive())
            {
                itemPopUp.gameObject.SetActive(false);
            }
        }
    }

    #region Character(Theif, NPC) Pop-up
    public GameObject charPopUp;
    
    public void SetCharPopUp(int playerID, Vector3 objPos)
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
        charPopUp.transform.position = screenPoint;
        charPopUp.SetActive(true);
    }
    #endregion

    #region Item Pop-up
    public ItemPopUp itemPopUp;

    public void SetItemPopUp(string [] attributes, Vector3 objPos)
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(objPos);
        //itemPopUp.gameObject.
        itemPopUp.transform.position = screenPoint;
        itemPopUp.SetAttributes(attributes);
        itemPopUp.gameObject.SetActive(true);
    }
    #endregion
}