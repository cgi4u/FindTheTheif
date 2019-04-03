using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;

namespace com.MJT.FindTheThief
{
    public class CircleMoveButton : MonoBehaviour
    {
        private float radius;
        Image image;
        private void Awake()
        {
            image = GetComponent<Image>();
            radius = Mathf.Min(GetComponent<RectTransform>().rect.height,
                                GetComponent<RectTransform>().rect.width) / 2;
        }

        public float minTouchRatio;
        public float maxTouchRatio;

        public Sprite defaultSprite;
        public Sprite downSprite;
        public Sprite rightSprite;
        public Sprite upSprite;
        public Sprite leftSprite;

        [SerializeField]
        private EMoveDirection curDirection = EMoveDirection.Stop;
        private void Update()
        {
            if (Input.GetMouseButton(0)
                && Vector2.Distance(transform.position, Input.mousePosition) / radius >= minTouchRatio
                && Vector2.Distance(transform.position, Input.mousePosition) / radius <= maxTouchRatio)
            {
                float angle = Mathf.Atan2(Input.mousePosition.y - transform.position.y,
                                            Input.mousePosition.x - transform.position.x) * Mathf.Rad2Deg;

                if (angle >= -135f && angle < -45f && curDirection != EMoveDirection.Down)
                {
                    image.sprite = downSprite;
                    OnDirectionChange(EMoveDirection.Down);
                }
                else if (angle >= -45f && angle < 45f && curDirection != EMoveDirection.Right)
                {
                    image.sprite = rightSprite;
                    OnDirectionChange(EMoveDirection.Right);
                }
                else if (angle >= 45f && angle < 135f && curDirection != EMoveDirection.Up)
                {
                    image.sprite = upSprite;
                    OnDirectionChange(EMoveDirection.Up);
                }
                else if ((angle >= 135f || angle < -135f) && curDirection != EMoveDirection.Left)
                {
                    image.sprite = leftSprite;
                    OnDirectionChange(EMoveDirection.Left);
                }
            }
            else if (curDirection != EMoveDirection.Stop)
            {
                image.sprite = defaultSprite;
                OnDirectionChange(EMoveDirection.Stop);
            }
        }

        private void OnDirectionChange(EMoveDirection direction)
        {
            curDirection = direction;
            PlayerController.LocalPlayer.ChangeMoveDirection(curDirection);
        }
    }
}
