using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{ 
    public class SkillSelector : MonoBehaviour
    {
        public SkillDataSet skillDataSet;
        private List<int> selectedSkills = new List<int>();

        [SerializeField]
        private SkillSelectButton[] buttons;

        private void Awake()
        {
            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                int prefSkill = PlayerPrefs.GetInt(skillDataSet.Team + " Skill " + i, -1);
                if (prefSkill == -1) break;

                selectedSkills.Add(prefSkill);
            }

            for (int i = 0; i < buttons.Length; i++)
            {
                SkillData data = skillDataSet.Get(i);
                bool isOn = selectedSkills.Contains(data.Code);

                buttons[i].SetSkillData(skillDataSet.Get(i), this, isOn);
            }

            RenewSkillLabel();
        }

        public bool OnOffSkill(int skillCode)
        {
            bool ret;

            if (!selectedSkills.Contains(skillCode))
            {
                if (selectedSkills.Count >= Constants.maxSkillNum)
                    return false;

                selectedSkills.Add(skillCode);
                ret = true;
            }
            else
            {
                selectedSkills.Remove(skillCode);
                ret = false;
            }

            RenewSkillLabel();
            return ret;
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
                PlayerPrefs.SetInt(skillDataSet.Team + " Skill " + i, selectedSkills[i]);
            }
            return true;
        }
    }
}
