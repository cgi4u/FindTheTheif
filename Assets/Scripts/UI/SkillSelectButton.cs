using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(Button))]
    public class SkillSelectButton : MonoBehaviour
    {
        private SkillData skillData;

        public void SetSkillData(SkillData _skillData)
        {
            skillData = _skillData;

            GetComponent<Image>().sprite = skillData.Icon;
        }

        
    }
}
