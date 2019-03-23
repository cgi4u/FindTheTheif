using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public enum SkillType { Passive, Time, Number }

    public class SkillData
    {
        static readonly string skillIconPath = "Assets\\Sprite\\UI\\Skill Icons\\";
        static SkillData[] allSkillData = {
            new SkillData(0, Resources.Load<Sprite>(skillIconPath + "Smoke"), SkillType.Number, 2)
        };

        public static SkillData Get(int skillCode)
        {
            for (int i = 0; i < allSkillData.Length; i++)
            {
                if (skillCode == allSkillData[i].code)
                    return allSkillData[i];
            }
            return null;
        }

        private int code;
        public int Code
        {
            get
            {
                return code;
            }
        }

        private Sprite icon;
        public Sprite Icon
        {
            get
            {
                return icon;
            }
        }

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
        private int typeData;
        public int TypeData
        {
            get
            {
                return typeData;
            }
        }

        SkillData(int _code, Sprite _icon, SkillType _type, int _typeData)
        {
            code = _code;
            icon = _icon;
            type = _type;
            typeData = _typeData;
        }
    }
}