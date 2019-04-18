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
        Sensing,
        TrapItem
    }

    /// <summary>
    /// Generate skill class instances.
    /// </summary>
    public static class SkillFactory
    { 
        public static Skill GetSkill(ESkillName name, SkillUseButton button)
        {
            switch (name) {
                case ESkillName.Arrest:
                    return new Arrest(button);
                case ESkillName.FastMove:
                    return new FastMove(button);
                case ESkillName.TrapItem:
                    return new TrapItem(button);
                case ESkillName.Sensing:
                    return new Sensing(button);
                case ESkillName.SecretPath:
                    return new SecretPath(button);
                case ESkillName.Smoke:
                    return new Smoke(button);
                default:
                    return new DummySkill(button);
            }
        }
    }

    /// <summary>
    /// Class that implements skill effect. Should be used like composition with a SkillUseButton.
    /// </summary>
    public abstract class Skill
    {
        protected SkillUseButton button;
        public Skill(SkillUseButton _button)
        {
            button = _button;
        }

        public abstract void Activate();
    }

    #region Detetive Skills

    public class Arrest : Skill
    {
        readonly int initCount = 1;
        public Arrest(SkillUseButton _button) : base(_button)
        {
            _button.SetPassive();
            MultiplayRoomManager.Instance.InitArrestSetting(_button, initCount);
        }

        public override void Activate()
        {

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

    public class TrapItem : Skill
    {
        public TrapItem(SkillUseButton _button) : base(_button)
        {
            button.SetRemainingCount(count);
        }

        int count = 2;
        public override void Activate()
        {
            ItemController.ActivatePickModeForAllItems(PrintItemName);
        }

        private void PrintItemName(ItemController item)
        {
            item.SetTrap();
            ItemController.DeactivatePickModeForAllItems();
            button.SetRemainingCount(--count);
        }
    }

    #endregion

    #region Thief Skills

    public class Sensing : Skill
    {
        public Sensing(SkillUseButton _button) : base(_button)
        { }

        public override void Activate()
        {
            DetectiveController.SetAlertForAllInstances(10f);
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

    #endregion

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
