using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public enum SkillType { Passive, Time, Number }

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
        }

        [SerializeField]
        private string methodName;
        public string MethodName
        {
            get
            {
                return methodName;
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

        [SerializeField]
        private SkillType type;
        public SkillType Type
        {
            get
            {
                return type;
            }
        }

        /// <summary>
        /// Timestamp for time type, limit skill using number of num type.
        /// </summary>
        [SerializeField]
        private int typeData;
        public int TypeData
        {
            get
            {
                return typeData;
            }
        }
    }
}
