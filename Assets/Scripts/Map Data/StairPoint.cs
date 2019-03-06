﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class StairPoint : MonoBehaviour
    {
        [SerializeField]
        private Transform linkedPoint;
        public Vector2 LinkedPoint
        {
            get
            {
                return linkedPoint.position;
            }
        }
    }
}