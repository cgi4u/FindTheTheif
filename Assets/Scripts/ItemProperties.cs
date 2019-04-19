using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public static class ItemProperties
    {
        public static readonly int ItemPropTypeNum = 3;
        public static readonly int ItemPropNumPerType = 3;

        public enum EItemColor { Red, Blue, Yellow }

        public static Color UIColorForProp(EItemColor color)
        {
            switch (color)
            {
                case EItemColor.Red:
                    return new Color(0.67f, 0.2f, 0.2f);
                case EItemColor.Blue:
                    return new Color(0.25f, 0.25f, 0.45f);
                case EItemColor.Yellow:
                    return new Color(0.91f, 0.76f, 0.08f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public static string UITextForProp(EItemColor color, bool shortMode)
        {
            string propString;
            switch (color)
            {
                case EItemColor.Red:
                    propString = "빨강";
                    break;
                case EItemColor.Blue:
                    propString = "파랑";
                    break;
                case EItemColor.Yellow:
                    propString = "노랑";
                    break;
                default:
                    propString = "오류";
                    break;
            }

            if (shortMode) propString = propString.Substring(0, 1);
            return propString;
        }

        public static int CodeForProp(EItemColor color)
        {
            switch (color)
            {
                case EItemColor.Red:
                    return 0;
                case EItemColor.Blue:
                    return 1;
                case EItemColor.Yellow:
                    return 2;
                default:
                    return -1;
            }
        }

        public enum EItemAge { Ancient, Middle, Modern }

        public static Color UIColorForProp(EItemAge age)
        {
            switch (age)
            {
                case EItemAge.Ancient:
                    return new Color(0.7f, 0.62f, 0.36f);
                case EItemAge.Middle:
                    return new Color(0.15f, 0.74f, 0.24f);
                case EItemAge.Modern:
                    return new Color(0.64f, 0.65f, 0.63f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public static string UITextForProp(EItemAge age, bool shortMode)
        {
            string propString;
            switch (age)
            {
                case EItemAge.Ancient:
                    propString = "고대";
                    break;
                case EItemAge.Middle:
                    propString = "중근세";
                    break;
                case EItemAge.Modern:
                    propString = "현대";
                    break;
                default:
                    propString = "오류";
                    break;
            }

            if (shortMode) propString = propString.Substring(0, 1);
            return propString;
        }

        public static int CodeForProp(EItemAge age)
        {
            switch (age)
            {
                case EItemAge.Ancient:
                    return ItemPropNumPerType;
                case EItemAge.Middle:
                    return ItemPropNumPerType + 1;
                case EItemAge.Modern:
                    return ItemPropNumPerType + 2;
                default:
                    return -1;
            }
        }

        public enum EItemUsage { Art, Daily, War }

        public static Color UIColorForProp(EItemUsage usage)
        {
            switch (usage)
            {
                case EItemUsage.Art:
                    return new Color(0.91f, 0.47f, 0.12f);
                case EItemUsage.Daily:
                    return new Color(0.29f, 0.71f, 0.89f);
                case EItemUsage.War:
                    return new Color(0.35f, 0.35f, 0.35f);
                default:
                    return new Color(1.0f, 1.0f, 1.0f);
            }
        }

        public static string UITextForProp(EItemUsage usage, bool shortMode)
        {
            string propString;
            switch (usage)
            {
                case EItemUsage.Art:
                    propString = "예술";
                    break;
                case EItemUsage.Daily:
                    propString = "일상";
                    break;
                case EItemUsage.War:
                    propString = "전쟁";
                    break;
                default:
                    propString = "오류";
                    break;
            }

            if (shortMode) propString = propString.Substring(0, 1);
            return propString;
        }

        public static int CodeForProp(EItemUsage usage)
        {
            switch (usage)
            {
                case EItemUsage.Art:
                    return 2 * ItemPropNumPerType;
                case EItemUsage.Daily:
                    return 2 * ItemPropNumPerType + 1;
                case EItemUsage.War:
                    return 2 * ItemPropNumPerType + 2;
                default:
                    return -1;
            }
        }
    }
}
