﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    /*
    [RequireComponent(typeof(Button))]
    public class SkillUseButtonLegacy : MonoBehaviour
    {
        public Sprite usableSprite;
        public Sprite unusableSprite;

        public Image skillIcon;

        static float imageOffsetRatio = 0.25f;
        Vector3 imageOffsetVec;

        SkillButtonInitializer skillManager;
        SkillData skillData;
        int skillIdx;
        bool usable = false;

        private void Awake()
        {
            imageOffsetVec = new Vector3(0f, GetComponent<RectTransform>().rect.height * imageOffsetRatio, 0f);
        }

        int skillCount;
        int skillDelayTimestamp;
        public void Init(SkillButtonInitializer _skillManager, SkillData _skillData, int _skillIdx)
        {
            skillManager = _skillManager;
            skillData = _skillData;
            skillIdx = _skillIdx;

            skillIcon.sprite = skillData.Icon;
            usable = true;
            remainingDelayText.gameObject.SetActive(false);
            
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
                    SetUnusableWithCount(skillCount);
                    break;
                case SkillType.Delay:
                    SetUnusableWithTime(skillDelayTimestamp);
                    break;
            }
        }

        public void OnTouchDown()
        {
            if (usable == false)
                return;

            GetComponent<Image>().sprite = unusableSprite;
            skillIcon.rectTransform.position -= imageOffsetVec;
            remainingCountText.rectTransform.position -= imageOffsetVec;
        }

        public void OnTouchUp()
        {
            if (usable == false)
                return;


            if (usable)
            {
                GetComponent<Image>().sprite = usableSprite;
                skillIcon.rectTransform.position += imageOffsetVec;
                remainingCountText.rectTransform.position += imageOffsetVec;
            }
        }

        public Text remainingDelayText;

        private void SetUnusableWithTime(int timestamp)
        {
            usable = false;

            StartCoroutine(SetUsableAfterTime((float)timestamp / 1000f));
        }

        private IEnumerator SetUsableAfterTime(float time)
        {
            remainingDelayText.gameObject.SetActive(true);

            while (time > 0f) {
                remainingDelayText.text = ((int)time).ToString();
                if (time > 1f)
                {
                    yield return new WaitForSeconds(1f);
                    time -= 1f;
                }
                else
                {
                    yield return new WaitForSeconds(time);
                    time -= time;
                }
            }

            remainingDelayText.gameObject.SetActive(false);
            usable = true;
            GetComponent<Image>().sprite = usableSprite;
            skillIcon.rectTransform.position += imageOffsetVec;
        }

        public Text remainingCountText;

        private void SetUnusableWithCount(int count)
        {
            remainingCountText.text = count.ToString();

            if (count == 0)
            {
                usable = false;
            }
        }

        
    }
    */
}
