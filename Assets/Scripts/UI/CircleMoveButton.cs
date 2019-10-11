using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    public class SpriteForDirection: SerializableDictionary<EMoveDirection, Sprite> { }

    public class CircleMoveButton: MonoBehaviour
    {
        private float radius;
        Image image;

        public float minTouchRatio;
        public float maxTouchRatio;

        public Sprite stopSprite;
        public Sprite downSprite;
        public Sprite rightSprite;
        public Sprite upSprite;
        public Sprite leftSprite;

        [SerializeField]
        private SpriteForDirection spriteForDirection;

        [SerializeField]
        private EMoveDirection currentDirection;
        private EMoveDirection CurrentDirection
        {
            get
            {
                return this.currentDirection;
            }

            set
            {
                if (this.currentDirection == value) return;
                this.currentDirection = value;
                //this.image.sprite = spriteForDirection[0];
                PlayerController.LocalPlayer.ChangeMoveDirection(value);
            }
        }

        private void Awake()
        {
            image = GetComponent<Image>();
            radius = Mathf.Min(GetComponent<RectTransform>().rect.height,
                                GetComponent<RectTransform>().rect.width) / 2;
        }

        private void Update()
        {
            if (!Input.GetMouseButton(0))
            {
                CurrentDirection = EMoveDirection.Stop;
                return;
            }

            float touchRatio = Vector2.Distance(transform.position, Input.mousePosition) / radius;
            if (touchRatio < minTouchRatio || touchRatio > maxTouchRatio)
                return;

            float angle = GlobalFunctions.GetAngle(Input.mousePosition, transform.position);

            if (angle >= -135f && angle < -45f)
                CurrentDirection = EMoveDirection.Down;
            else if (angle >= -45f && angle < 45f)
                CurrentDirection = EMoveDirection.Right;
            else if (angle >= 45f && angle < 135f)
                CurrentDirection = EMoveDirection.Up;
            else if (angle >= 135f || angle < -135f)
                CurrentDirection = EMoveDirection.Left;
        }
    }
}
