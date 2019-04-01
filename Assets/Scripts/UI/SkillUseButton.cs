using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(Button))]
    public class SkillUseButton : MonoBehaviour
    {
        public Sprite usableSprite;
        public Sprite unusableSprite;

        public Image skillIcon;

        static float imageOffsetRatio = 0.25f;
        Vector3 imageOffsetVec;

        SkillManager skillManager;
        SkillData skillData;
        int skillIdx;
        bool usable = false;

        private void Awake()
        {
            imageOffsetVec = new Vector3(0f, GetComponent<RectTransform>().rect.height * imageOffsetRatio, 0f);
        }

        int skillCount;
        int skillDelayTimestamp;
        public void Init(SkillManager _skillManager, SkillData _skillData, int _skillIdx)
        {
            skillManager = _skillManager;
            skillData = _skillData;
            skillIdx = _skillIdx;

            skillIcon.sprite = skillData.Icon;
            usable = true;

            switch (skillData.Type)
            {
                case SkillType.Count:
                    skillCount = skillData.TypeData;
                    break;
                case SkillType.Delay:
                    skillDelayTimestamp = skillData.TypeData;
                    break;
                case SkillType.Passive:
                    skillManager.UseSkill(skillIdx);
                    usable = false;
                    break;
            }
        }

        public void OnTouched()
        {
            if (usable == false)
                return;

            skillManager.UseSkill(skillIdx);
            switch (skillData.Type)
            {
                case SkillType.Count:
                    skillCount -= 1;
                    if (skillCount == 0)
                    {
                        usable = false;
                    }
                    break;
                case SkillType.Delay:
                    SetUnusableWithTime(skillDelayTimestamp);
                    break;
            }
        }

        public Text remainingDelayText;

        private void SetUnusableWithTime(int timestamp)
        {
            usable = false;
            GetComponent<Image>().sprite = unusableSprite;
            skillIcon.rectTransform.position = skillIcon.rectTransform.position - imageOffsetVec;

            StartCoroutine(SetUsableAfterTime((float)timestamp / 1000f));
        }

        private IEnumerator SetUsableAfterTime(float time)
        {
            yield return new WaitForSeconds(time);

            usable = true;
            GetComponent<Image>().sprite = usableSprite;
            skillIcon.rectTransform.position = skillIcon.rectTransform.position + imageOffsetVec;
        }

        public Text remainingCountText;

        private void SetUnsuableWithCount(int Count)
        {

        }
    }
}
