using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [Serializable]
    public class SkillData
    {
        [SerializeField]
        private int code;
        public int Code
        {
            get
            {
                return code;
            }
            set
            {
                code = value;
            }
        }

        [SerializeField]
        private ESkillName skillName;
        public ESkillName SkillName
        {
            get
            {
                return skillName;
            }
        }

        [SerializeField]
        private Sprite icon;
        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }

        
    }
}
