using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class SkillManager : MonoBehaviour
    {
        private int[] skills = new int[Constants.maxSkillNum];
        void Start()
        {
            if (!PhotonNetwork.connected)
                Destroy(this.gameObject);

            for (int i = 0; i < Constants.maxSkillNum; i++)
            {
                skills[i] = PlayerPrefs.GetInt(MultiplayRoomManager.Instance.MyTeam + " Skill " + i, -1);
                if (skills[i] == -1)
                {
                    Debug.LogError("Player Skills are not set properly.");
                    return;
                }

                Debug.Log(skills[i]);
            }
        }
    }
}