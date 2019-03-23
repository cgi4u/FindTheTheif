using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(Button))]
    public class SkillSelectButton : MonoBehaviour
    {
        public SkillManager skillManager;
        public int skillCode;
        private void Awake()
        {
            
        }
    }
}
