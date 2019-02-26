using System.Collections;
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

        [SerializeField]
        private Vector2 defaultOffset;
        public Vector2 DefaultPos
        {
            get
            {
                return (Vector2)transform.position + defaultOffset;
            }
        }
    }
}
