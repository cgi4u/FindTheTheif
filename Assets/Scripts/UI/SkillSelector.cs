using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{ 
    public class SkillSelector : MonoBehaviour
    {
        public Team team;
        private List<int> enabledSkills = new List<int>();

        private void Awake()
        {
            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                int prefSkill = PlayerPrefs.GetInt(team + " Skill " + i, -1);
                if (prefSkill == -1) return;

                enabledSkills.Add(prefSkill);
            }

            RenewSkillLabel();
        }

        public void SkillOnOff(int skillNum)
        {
            if (!enabledSkills.Contains(skillNum))
            {
                if (enabledSkills.Count >= Constants.maxSkillNum)
                    return;

                enabledSkills.Add(skillNum);
            }
            else
            {
                enabledSkills.Remove(skillNum);
            }

            RenewSkillLabel();
        }

        public Text skillLabel;
        public void RenewSkillLabel()
        {
            skillLabel.text = "";
            for (int i = 0; i < enabledSkills.Count; i++)
            {
                skillLabel.text += enabledSkills[i];
                if (i != enabledSkills.Count - 1)
                    skillLabel.text += ", ";
            }
        }

        public bool SetSkillPreference()
        {
            if (Constants.maxSkillNum != enabledSkills.Count)
                return false;

            for (int i = 0; i < enabledSkills.Count; i++)
            {
                PlayerPrefs.SetInt(team + " Skill " + i, enabledSkills[i]);
            }
            return true;
        }
    }
}
