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
        Vector3 imageOffsetVec;

        public Text remainingCountText;
        public Text remainingDelayText;

        bool usable = false;
        Button button;

        Skill enabledSkill;

        protected void Awake()
        {
            imageOffsetVec = new Vector3(0f, GetComponent<RectTransform>().rect.height * imageOffsetRatio, 0f);
            button = GetComponent<Button>();
        }

        public void Init(SkillData _skillData)
        {
            skillData = _skillData;
            skillIcon.sprite = skillData.Icon;

            enabledSkill = SkillFactory.GetSkill(skillData.SkillName, this);
        }

        public void SetPassive()
        {
            button.interactable = false;
        }

        public void SetButtonDown()
        {
            GetComponent<Image>().sprite = unusableSprite;
            skillIcon.rectTransform.position -= imageOffsetVec;
            remainingCountText.rectTransform.position -= imageOffsetVec;
        }

        public void ActivateSkill()
        {
            enabledSkill.Activate();
        }

        public void SetButtonUp()
        {
            GetComponent<Image>().sprite = usableSprite;
            skillIcon.rectTransform.position += imageOffsetVec;
            remainingCountText.rectTransform.position += imageOffsetVec;
        }

        public void SetRemainingCount(int count)
        {
            remainingCountText.text = count.ToString();
            remainingCountText.gameObject.SetActive(true);
            if (count == 0)
            {
                button.interactable = false;
                SetButtonDown();
            }
        }

        public void SetRemainingDelayTime(int seconds)
        {
            remainingDelayText.gameObject.SetActive(true);
            button.interactable = false;
            SetButtonDown();

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
            button.interactable = true;
            SetButtonUp();
        }
    }
}