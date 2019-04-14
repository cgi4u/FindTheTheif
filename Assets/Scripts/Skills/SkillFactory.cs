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
        SecretPath,
        Arrest,
        Sensing
    }

    public static class SkillFactory
    { 
        public static Skill GetSkill(ESkillName name, SkillUseButton button)
        {
            switch (name) {
                case ESkillName.FastMove:
                    return new FastMove(button);
                case ESkillName.SecretPath:
                    return new SecretPath(button);
                case ESkillName.Smoke:
                    return new Smoke(button);
                case ESkillName.Sensing:
                    return new Sensing(button);
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

    public class Sensing : Skill
    {
        public Sensing(SkillUseButton _button) : base(_button)
        { }

        public override void Activate()
        {
            ThiefController.LocalThief.SetSensingDuringSeconds(1000f);
            button.SetRemainingDelayTime(30);
        }
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

    public class SecretPath : Skill
    {
        public SecretPath(SkillUseButton _button) : base(_button)
        {
            button.SetRemainingCount(count / 2);
        }

        int count = 2;
        public override void Activate()
        {
            if (ThiefController.LocalThief.MakeSmoke())
            {
                count -= 1;
                button.SetRemainingCount(count / 2 + count % 2);
            }
        }
    }

    public class Smoke : Skill
    {
        public Smoke(SkillUseButton _button) : base(_button)
        {
        }

        public override void Activate()
        {
            if (ThiefController.LocalThief.MakeSmoke())
            {
                button.SetRemainingDelayTime(30);
            }
        }
    }

    public class DummySkill : Skill
    {
        public DummySkill(SkillUseButton _button) : base(_button)
        {
            button.SetRemainingCount(count);
        }

        int count = 5;
        public override void Activate()
        {
            Debug.Log("Dummy Method");
            button.SetRemainingCount(--count);
        }
    }
}
