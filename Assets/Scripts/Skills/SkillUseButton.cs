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

        SkillData skillData;

        public Image skillIcon;
        static float imageOffsetRatio = 0.25f;

        Vector3 iconUpPos;
        Vector3 iconDownPos;
        Vector3 countUpPos;
        Vector3 countDownPos;

        public Text remainingCountText;
        public Text remainingDelayText;

        [SerializeField]
        bool usable = false;

        Skill enabledSkill;

        protected void Awake()
        {
            Vector3 imageOffsetVec = new Vector3(0f, GetComponent<RectTransform>().rect.height * imageOffsetRatio, 0f);

            iconUpPos = skillIcon.rectTransform.position;
            iconDownPos = skillIcon.rectTransform.position - imageOffsetVec;

            countUpPos = remainingCountText.rectTransform.position;
            countDownPos = remainingCountText.rectTransform.position - imageOffsetVec;
        }

        public void Init(SkillData _skillData)
        {
            skillData = _skillData;
            skillIcon.sprite = skillData.Icon;

            enabledSkill = SkillFactory.GetSkill(skillData.SkillName, this);

            usable = true;
        }

        public void SetPassive()
        {
            usable = false;
        }

        public void SetButtonDown(bool byTouch)
        {
            if (byTouch && !usable) return;

            GetComponent<Image>().sprite = unusableSprite;
            skillIcon.rectTransform.position = iconDownPos;
            remainingCountText.rectTransform.position = countDownPos;
        }

        public void ActivateSkill()
        {
            if (!usable) return;

            enabledSkill.Activate();
        }

        public void SetButtonUp(bool byTouch)
        {
            if (byTouch && !usable) return;

            GetComponent<Image>().sprite = usableSprite;
            skillIcon.rectTransform.position = iconUpPos;
            remainingCountText.rectTransform.position = countUpPos;
        }

        public void SetRemainingCount(int count)
        {
            remainingCountText.text = count.ToString();
            remainingCountText.gameObject.SetActive(true);
            if (count == 0)
            {
                SetButtonDown(false);
                usable = false;
            }
        }

        public void SetRemainingDelayTime(int seconds)
        {
            remainingDelayText.gameObject.SetActive(true);
            SetButtonDown(false);
            usable = false;

            StartCoroutine(SetActiveAfterSeconds(seconds));
        }

        private IEnumerator SetActiveAfterSeconds(int seconds)
        {
            while (seconds != 0)
            {
                remainingDelayText.text = seconds.ToString();
                yield return new WaitForSeconds(1);
                seconds--;
            }

            remainingDelayText.gameObject.SetActive(false);
            usable = true;
            SetButtonUp(false);
        }
    }
}