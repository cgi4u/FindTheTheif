using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [CreateAssetMenu]
    public class NPCPrefabSet : ScriptableObject
    {
        [SerializeField]
        private GameObject[] set;

        public GameObject Get(int i)
        {
            return set[i];
        }

        public int Count
        {
            get
            {
                return set.Length;
            }
        }
    }
}
