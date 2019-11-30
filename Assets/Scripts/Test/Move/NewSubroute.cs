using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class NewSubroute : MonoBehaviour
    {
        [SerializeField]
        private List<Transform> nodes;
        public List<Transform> Nodes => nodes;
    }
}
