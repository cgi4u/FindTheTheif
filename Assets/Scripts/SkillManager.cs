using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    
    public class SkillManager : MonoBehaviour
    {
        delegate void SkillMethod();

        private List<SkillMethod> thiefSkills = new List<SkillMethod>();
        public Sprite[] thiefSkillLogos;

        private List<SkillMethod> detectiveSkills = new List<SkillMethod>();
        public Sprite[] detectiveSkillLogos;

        private List<SkillMethod> skillMethods = new List<SkillMethod>();

        private void Awake()
        {
            Sprite a = Resources.Load<Sprite>("Assets/Sprite/UI/Skill Icons/Smoke.png");

            // Skill Method List Initializing
            thiefSkills.Add(Smoke);
            thiefSkills.Add(SecretDoor);

            detectiveSkills.Add(FastMoving);
            detectiveSkills.Add(ItemChange);

            skillMethods.Add(Smoke);
            skillMethods.Add(SecretDoor);
            skillMethods.Add(FastMoving);
            skillMethods.Add(ItemChange);
        }

        private int[] skillsInt = new int[Constants.maxSkillNum];
        private List<SkillMethod> enabledSkills = new List<SkillMethod>();
        public Button[] enabledSkillButtons;
        
        void Start()
        {

            if (!PhotonNetwork.connected)
                return;

            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                int selectedSkill = PlayerPrefs.GetInt(MultiplayRoomManager.Instance.MyTeam + " Skill " + i);

                if (MultiplayRoomManager.Instance.MyTeam == Team.Thief)
                {
                    enabledSkills.Add(thiefSkills[selectedSkill]);
                    enabledSkillButtons[i].image.sprite = thiefSkillLogos[selectedSkill];
                }
                else
                {
                    enabledSkills.Add(detectiveSkills[selectedSkill]);
                    enabledSkillButtons[i].image.sprite = detectiveSkillLogos[selectedSkill];
                }
            }
        }

        public void UseSkill(int skillNum)
        {
            enabledSkills[skillNum]();

        }

        #region Thief Skills

        private void Smoke()
        {
            Debug.Log("Use Smoke");
        }

        private void SecretDoor()
        {
            Debug.Log("Use SecretDoor");
        }

        #endregion

        #region Detective Skills()

        private void FastMoving()
        {
            Debug.Log("Use FastMoving");
        }

        private void ItemChange()
        {
            Debug.Log("Use ItemChange");
        }

        #endregion
    }
}