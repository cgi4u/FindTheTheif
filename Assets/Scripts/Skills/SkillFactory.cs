using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public enum ESkillName
    {
        FastMove,
        ItemChange,
        Smoke,
        SecretPath
    }

    public static class SkillFactory
    {
        public static Skill GetSkill(ESkillName name, SkillUseButton button)
        {
            switch (name) {
                case ESkillName.FastMove:
                    return new FastMove(button);
                default:
                    return new DummySkill(button);
            }
        }
    }

    public abstract class Skill
    {
        protected SkillUseButton button;
        public Skill(SkillUseButton _button)
        {
            button = _button;
        }

        public abstract void Activate();
    }

    public class FastMove : Skill
    {
        public FastMove(SkillUseButton _button) : base(_button)
        { }

        public override void Activate()
        {
            PlayerController.LocalPlayer.BoostSpeed(1.5f, 10);
            button.SetRemainingDelayTime(30);
        }
    }

    public class DummySkill : Skill
    {
        public DummySkill(SkillUseButton _button) : base(_button)
        { }

        int count = 5;
        public override void Activate()
        {
            Debug.Log("Dummy Method");
            button.SetRemainingCount(--count);
        }
    }
}
