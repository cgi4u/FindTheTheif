using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{

    abstract class MoveBehaviour
    {
        private static List<Vector2> obstaclePositions;

        protected float moveSpeed;

        protected Vector2 startPoint;
        protected Vector2 targetPoint;

        protected NewRoute route;

        protected int subRouteIndex;
        protected int nodeIndex;

        protected MoveBehaviour(float _moveSpeed)
        {
            moveSpeed = _moveSpeed;
        }

        private void Move(GameObject movingObject)
        {
            Transform transform = movingObject.transform;
            transform.position = Vector2.MoveTowards(transform.position, targetPoint, moveSpeed * Time.deltaTime);

            if (transform.position.Equals(targetPoint))
                OnArrivedToTargetPoint(movingObject);
            OnMoveCompleted();
        }

        protected abstract void OnArrivedToTargetPoint(GameObject movingObject);
        protected abstract void OnMoveCompleted();
    }

    class PlayerMoveBehaviour : MoveBehaviour
    {
        public EMoveDirection Direction { get; set; }

        private bool isMoving;

        PlayerMoveBehaviour(float _moveSpeed) : base(_moveSpeed) {}

        protected override void OnArrivedToTargetPoint(GameObject movingObject)
        {
            if (targetPoint == (Vector2)route.SubRoutes[subRouteIndex].Nodes[nodeIndex].position)
                nodeIndex += 1;

            if (nodeIndex != route.SubRoutes[subRouteIndex].Nodes.Count)
                subRouteIndex += 1;

            if (subRouteIndex != route.SubRoutes.Count)
                route = route.NextRoutes[Random.Range(0, route.NextRoutes.Count - 1)];

            targetPoint = targetPoint + EnumExtensions.VectorForDirection(Direction);

            // Collider box는 어떻게 얻어올건지...
            RaycastHit2D[] hits = Physics2D.BoxCastAll(startPoint, raycastBox, 0, targetPoint - startPoint, Vector2.Distance(startPoint, targetPoint));

            bool ifHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider.gameObject != movingObject && !hit.collider.isTrigger)
                {
                    //플레이어 오브젝트와 충돌체의 오브젝트가 같지 않는 상황, 즉 콜라이더를 갖는 다른 오브젝트에 부딫힌 상황
                    ifHit = true;
                    break;
                }
            }

            if (ifHit)
                isMoving = false;
            else
                isMoving = true;

            SetAnimationProperty();
        }

        protected override void OnMoveCompleted()
        {
            throw new System.NotImplementedException();
        }
    }
}