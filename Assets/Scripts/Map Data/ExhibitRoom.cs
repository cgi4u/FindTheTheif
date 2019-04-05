using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
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
        private int num;
        /// <summary>
        /// Room number used in game. Separated from the index of this room in the room list of MapDataManager.
        /// </summary>
        public int Num
        {
            get
            {
                return num;
            }
        }

         /*
        [SerializeField]
        private StairSide adjStairSide;
        public StairSide AdjStairSide
        {
            get
            {
                return adjStairSide;
            }
        }
        */

        //방 내부의 아이템 생성 지점
        [SerializeField]
        private ItemGenPoint[] itemGenPoints;
        public ItemGenPoint[] ItemGenPoints
        {
            get
            {
                return itemGenPoints;
            }
        }

        [SerializeField]
        private PutItemPoint[] putItemPoints;
        public PutItemPoint[] PutItemPoints
        {
            get
            {
                return putItemPoints;
            }
        }

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
        private Dictionary<int, List<Route>> toRoomRoutes_dic = new Dictionary<int, List<Route>>();
        public Dictionary<int, List<Route>> ToRoomRoutes
        {
            get
            {
                return toRoomRoutes_dic;
            }
        }

        [SerializeField]
        private StairRouteContainer fromStairRoutes;
        public StairRouteContainer FromStairRoutes
        {
            get
            {
                return fromStairRoutes;
            }
        }

        [SerializeField]
        private StairRouteContainer toStairRoutes;
        public StairRouteContainer ToStairRoutes
        {
            get
            {
                return toStairRoutes;
            }
        }

        private void Awake()
        {
           foreach (Route route in toRoomRoutes)
            {
                if (route.EndRoom < 0)
                {
                    Debug.LogError("End room of " + route.name + " is not set properly.");
                    return;
                }

                if (!toRoomRoutes_dic.ContainsKey(route.EndRoom))
                    toRoomRoutes_dic[route.EndRoom] = new List<Route>();
                toRoomRoutes_dic[route.EndRoom].Add(route);
            }
        }
    }
}
