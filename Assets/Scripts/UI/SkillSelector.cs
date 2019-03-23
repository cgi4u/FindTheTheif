using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{ 
    public class SkillSelector : MonoBehaviour
    {
        public Team team;
        private List<int> selectedSkills = new List<int>();

        private void Awake()
        {
            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                int prefSkill = PlayerPrefs.GetInt(team + " Skill " + i, -1);
                if (prefSkill == -1) return;

                selectedSkills.Add(prefSkill);
            }

            RenewSkillLabel();
        }

        public void SkillOnOff(int skillCode)
        {
            if (!selectedSkills.Contains(skillCode))
            {
                if (selectedSkills.Count >= Constants.maxSkillNum)
                    return;

                selectedSkills.Add(skillCode);
            }
            else
            {
                selectedSkills.Remove(skillCode);
            }

            RenewSkillLabel();
        }

        public Text skillLabel;
        public void RenewSkillLabel()
        {
            skillLabel.text = "";
            for (int i = 0; i < selectedSkills.Count; i++)
            {
                skillLabel.text += selectedSkills[i];
                if (i != selectedSkills.Count - 1)
                    skillLabel.text += ", ";
            }
        }

        /// <summary>
        /// Save skillset selected by user to PlayerPrefs. PlayerPrefs should be replaced by Game Server in release.
        /// </summary>
        /// <returns>Return true when success to save, or return false.</returns>
        public bool SetSkillPreference()
        {
            if (Constants.maxSkillNum != selectedSkills.Count)
                return false;

            for (int i = 0; i < selectedSkills.Count; i++)
            {
                PlayerPrefs.SetInt(team + " Skill " + i, selectedSkills[i]);
            }
            return true;
        }
    }
}
