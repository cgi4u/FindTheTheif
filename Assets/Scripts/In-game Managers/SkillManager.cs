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
        private Dictionary<int, SkillMethod> dectectiveSkillMethodMap;

        public SkillDataSet thiefSkillDataSet;
        private Dictionary<int, SkillMethod> thiefSkillMethodMap;

        private void Awake()
        {
            dectectiveSkillMethodMap = new Dictionary<int, SkillMethod>();
            for (int i = 0; i < detectiveSkillDataSet.DataCount(); i++)
            {
                MethodInfo methodInfo = GetType().GetMethod("Smoke", BindingFlags.NonPublic | BindingFlags.Instance);
                SkillMethod method = (SkillMethod)Delegate.CreateDelegate(typeof(SkillMethod), this, "Smoke", false);

                method();
            }

            thiefSkillMethodMap = new Dictionary<int, SkillMethod>();
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