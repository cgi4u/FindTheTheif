using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class TestNPCGenerator : MonoBehaviour
    {
        public GameObject NPCPrefab;

        void Start()
        {
            while (MapDataManager.Instance == null) ; 

            //Route Test
            for (int i = 0; i < 5; i++)
            {
                //Debug.Log("Random Node" + i);

                RouteNode randomPoint = MapDataManager.Instance.GetRandomGenerationPoint();
                if (randomPoint == null)
                {
                    Debug.LogError("Error: Attempt to generate more number of NPC than available");
                    return;
                }

                GameObject newNPC = Instantiate(NPCPrefab);
                newNPC.GetComponent<NPCController>().ManualStart(randomPoint);
            }
        }
    }
}
