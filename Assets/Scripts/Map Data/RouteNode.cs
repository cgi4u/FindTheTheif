﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    /// <summary>
    /// Store datas for item(If it is an item watching point or not, item direction)
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class RouteNode : MonoBehaviour
    {
        /// <summary>
        /// Position of this node in 2D space.
        /// </summary>
        public Vector2 position
        {
            get
            {
                return (Vector2)transform.position;
            }
        }

        [SerializeField]
        private bool isItemPoint;
        public bool IsItemPoint
        {
            get
            {
                return isItemPoint;
            }
        }

        [SerializeField]
        private Direction itemDir;
        public Direction ItemDir
        {
            get
            {
                return itemDir;
            }
        }

    }
}
