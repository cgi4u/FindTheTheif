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
        private SkillData[] set;
        [SerializeField]
        private ETeam team;
        public ETeam Team
        {
            get
            {
                return team;
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