using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // 각 전시실의 번호와 층을 저장
    // NPC가 이동시 계단을 경유해야 하는지 판단하기 위해 사용
    public class ExhibitRoom : MonoBehaviour
    {
        [SerializeField]
        private int floor;
        /// <summary>
        /// Floor number of room(0 -> 1st Floor)
        /// </summary>
        public int Floor
        {
            get
            {
                return floor;
            }
        }
        [SerializeField]
        private StairSide adjStairSide;
        public StairSide AdjStairSide
        {
            get
            {
                return adjStairSide;
            }
        }

        //방 내부의 아이템 생성 지점
        [SerializeField]
        private Transform[] itemGenPoints;
        public Transform[] ItemGenPoints
        {
            get
            {
                return itemGenPoints;
            }
        }

        //방과 관련한 경로들
        [SerializeField]
        private Route inRoomRoute;
        public Route InRoomRoute
        {
            get
            {
                return inRoomRoute;
            }
        }
        [SerializeField]
        private Route[] toRoomRoutes;
        public Route[] ToRoomRoutes
        {
            get
            {
                return toRoomRoutes;
            }
        }
        [SerializeField]
        private Route[] toStairRoutes;
        public Route[] ToStairRoutes
        {
            get
            {
                return toStairRoutes;
            }
        }
        [SerializeField]
        private Route[] fromStairRoutes;
        public Route[] FromStairRoutes
        {
            get
            {
                return fromStairRoutes;
            }
        }
    }
}
