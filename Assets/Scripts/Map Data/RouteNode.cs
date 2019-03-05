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
        private Vector2 itemDir;
        public Vector2 ItemDir
        {
            get
            {
                return itemDir;
            }
        }

        /*[SerializeField]
        private bool isStairPoint;
        public bool IsStairPoint
        {
            get
            {
                return isStairPoint;
            }
        }*/

        [SerializeField]
        private Vector2 defaultOffset;
        public Vector2 DefaultOffset
        {
            get
            {
                return defaultOffset;
            }
        }

        public Vector2 DefaultPos
        {
            get
            {
                return (Vector2)transform.position + defaultOffset;
            }
        }

        private void Awake()
        {
            if (!isItemPoint && itemDir != new Vector2())
            {
                Debug.LogError(gameObject.name + " of " + transform.parent.name + " is have itemDir value but it's not a item watching point. It may be default position value.");
            }
        }
    }
}
