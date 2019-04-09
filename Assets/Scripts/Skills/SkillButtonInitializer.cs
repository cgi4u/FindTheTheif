using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    public class SkillButtonInitializer : MonoBehaviour
    {

        delegate void SkillMethod();

        public SkillDataSet detectiveSkillDataSet;
        private List<SkillMethod> detectiveSkillMethods;

        public SkillDataSet thiefSkillDataSet;
        private List<SkillMethod> thiefSkillMethods;

        private void Awake()
        {

        }

        public SkillUseButton[] enabledSkillButtons;
        
        void Start()
        {

            if (!PhotonNetwork.connected)
                return;

            if (enabledSkillButtons.Length != Constants.maxSkillNum)
            {
                Debug.LogError("Number of skill buttons must be same with the maximum number of usable skill in a game.");
                Destroy(this.gameObject);
                return;
            }
        }

        bool initilaized = false;

        private void Update()
        {
            if (initilaized || MultiplayRoomManager.Instance == null)
                return;

            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                //Get saved skill code
                int selectedSkillIdx = PlayerPrefs.GetInt(MultiplayRoomManager.Instance.MyTeam + " Skill " + i);

                SkillData selectedSkillData;
                if (MultiplayRoomManager.Instance.MyTeam == ETeam.Detective)
                    selectedSkillData = detectiveSkillDataSet.Get(selectedSkillIdx);
                else
                    selectedSkillData = thiefSkillDataSet.Get(selectedSkillIdx);

                enabledSkillButtons[i].Init(selectedSkillData);
            }

            initilaized = true;
        }
    }
}