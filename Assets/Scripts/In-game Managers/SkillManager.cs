using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    
    public class SkillManager : MonoBehaviour
    {
        delegate void SkillMethod();

        public SkillDataSet detectiveSkillDataSet;
        private List<SkillMethod> detectiveSkillMethods;

        public SkillDataSet thiefSkillDataSet;
        private List<SkillMethod> thiefSkillMethods;

        private void Awake()
        {
            detectiveSkillMethods = new List<SkillMethod>();
            for (int i = 0; i < detectiveSkillDataSet.DataCount(); i++)
            {
                SkillData data = detectiveSkillDataSet.Get(i);
                SkillMethod method = (SkillMethod)Delegate.CreateDelegate(typeof(SkillMethod), this, data.MethodName, false);
                detectiveSkillMethods.Add(method);
            }

            thiefSkillMethods = new List<SkillMethod>();
            for (int i = 0; i < thiefSkillDataSet.DataCount(); i++)
            {
                SkillData data = thiefSkillDataSet.Get(i);
                SkillMethod method = (SkillMethod)Delegate.CreateDelegate(typeof(SkillMethod), this, data.MethodName, false);
                thiefSkillMethods.Add(method);
            }
        }

        public SkillUseButton[] enabledSkillButtons;
        
        void Start()
        {

            if (!PhotonNetwork.connected)
                return;

            if (enabledSkillButtons.Length != Constants.maxSkillNum)
            {
                Debug.LogError("Number of skill buttons must be same with the maximum number of usable skill in a game.");
                return;
            }

            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                //Get saved skill code
                int selectedSkillIdx = PlayerPrefs.GetInt(MultiplayRoomManager.Instance.MyTeam + " Skill " + i);

                //여기서 스킬데이터를 보내려고 해도 바로 접근할 수 있는 방법이 없음
                //skilldata의 인덱스와 동일하게 저장한후에 바로 접근할수있도록?

                SkillData selectedSkillData;
                if (MultiplayRoomManager.Instance.MyTeam == ETeam.Detective)
                    selectedSkillData = detectiveSkillDataSet.Get(selectedSkillIdx);
                else
                    selectedSkillData = thiefSkillDataSet.Get(selectedSkillIdx);

                enabledSkillButtons[i].Init(this, selectedSkillData, selectedSkillIdx);
            }
        }

        public void UseSkill(int idx)
        {
            if (MultiplayRoomManager.Instance.MyTeam == ETeam.Detective)
                detectiveSkillMethods[idx]();
            else
                thiefSkillMethods[idx]();
        }

        #region Thief Skills

        private void Smoke()
        {
            Debug.Log("Use Smoke");
        }

        private void SecretPath()
        {
            Debug.Log("Use SecretPath");
        }

        #endregion

        #region Detective Skills()

        private void FastMove()
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