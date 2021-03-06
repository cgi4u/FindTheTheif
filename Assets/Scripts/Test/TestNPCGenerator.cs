﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class TestNPCGenerator : MonoBehaviour
    {
        public GameObject NPCPrefab;
        public int NPCNumber;

        void Start()
        {
            while (MapDataManager.Instance == null) ; 

            //Route Test
            for (int i = 0; i < NPCNumber; i++)
            {
                int randomPointIdx = MapDataManager.Instance.GetRandomNPCGenPoint();
                if (randomPointIdx == -1)
                {
                    Debug.LogError("Error: Attempt to generate more number of NPC than available");
                    return;
                }

                GameObject newNPC = Instantiate(NPCPrefab);
                newNPC.name = "NPC " + i;
                newNPC.transform.parent = transform;
                newNPC.GetComponent<NPCController>().Init(randomPointIdx);
                newNPC.GetComponent<NPCController>().Activated = true;
            }
        }
    }
}
