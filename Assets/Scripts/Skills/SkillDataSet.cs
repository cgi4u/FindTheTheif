using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    [CreateAssetMenu]
    public class SkillDataSet : ScriptableObject
    {
        [SerializeField]
        private ETeam team;
        public ETeam Team
        {
            get
            {
                return team;
            }
        }

        [SerializeField]
        private SkillData defaultSkill;
        public SkillData DefaultSkill
        {
            get
            {
                return defaultSkill;
            }
        }

        [SerializeField]
        private SkillData[] set;
        

        private void Awake()
        {
            for (int i = 0; i < set.Length; i++)
            {
                set[i].Code = i;
            }
        }

        public SkillData Get(int i)
        {
            if (i < 0 || i >= set.Length)
                return null;

            return set[i];
        }

        public int DataCount()
        {
            return set.Length;
        }
    }
}