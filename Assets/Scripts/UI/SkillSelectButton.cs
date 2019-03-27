using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    [RequireComponent(typeof(Button))]
    public class SkillSelectButton : MonoBehaviour
    {
        public Sprite offSprite;
        public Sprite onSprite;

        public Image skillIcon;

        static float imageOffsetRatio = 0.25f;
        private Vector3 imageOffsetVec;

        private SkillData skillData;
        private SkillSelector skillSelector;

        private bool initialized = false;

        private void Awake()
        {
            imageOffsetVec = new Vector3(0f, GetComponent<RectTransform>().rect.height * imageOffsetRatio, 0f);
        }

        public void SetSkillData(SkillData _skillData, SkillSelector _skillSelector, bool isOn)
        {
            skillData = _skillData;
            skillSelector = _skillSelector;

            skillIcon.sprite = skillData.Icon;

            if (isOn)
            {
                GetComponent<Image>().sprite = onSprite;
                skillIcon.rectTransform.position = skillIcon.rectTransform.position - imageOffsetVec;
            }

            initialized = true;
        }

        public void OnTouched()
        {
            if (!initialized) return;

            if (skillSelector.OnOffSkill(skillData.Code))
            {
                if (GetComponent<Image>().sprite == offSprite)
                    skillIcon.rectTransform.position = skillIcon.rectTransform.position - imageOffsetVec;
                GetComponent<Image>().sprite = onSprite;
            }
            else
            {
                if (GetComponent<Image>().sprite == onSprite)
                    skillIcon.rectTransform.position = skillIcon.rectTransform.position + imageOffsetVec;
                GetComponent<Image>().sprite = offSprite;
            }
        }
    }
}
