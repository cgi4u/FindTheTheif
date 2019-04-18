using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheThief
{   
    /// <summary>
    /// Serialize constant datas of a map, and make other classes be able to get them by using singleton object.
    /// </summary>
    public class MapDataManager : MonoBehaviour
    {
        //Singleton Instance
        private static MapDataManager instance;
        public static MapDataManager Instance
        {
            get
            {
                return instance;
            }
        }

        #region Map Object Properties

        [SerializeField]
        private List<ExhibitRoom> rooms;
        /// <summary>
        /// Room datas(should be serialized)
        /// </summary>
        public List<ExhibitRoom> Rooms
        {
            get
            {
                return rooms;
            }
        }

        private List<ItemGenPoint> itemGenPoints = new List<ItemGenPoint>();
        /// <summary>
        /// Points where items can be generated
        /// </summary>
        public List<ItemGenPoint> ItemGenPoints
        {
            get
            {
                return itemGenPoints;
            }
        }

        private List<RouteNode> nPCGenPoints = new List<RouteNode>();
        /// <summary>
        /// RouteNodes where NPC can be generated.
        /// </summary>
        public List<RouteNode> NPCGenPoints
        {
            get
            {
                return nPCGenPoints;
            }
        }

        [SerializeField]
        private List<StairRouteContainer> stairToStairRoutes;
        /// <summary>
        /// Stair to Stair Routes. 
        /// Since the first and the last floor don't have Stair to Stair Routes, the number of container is (roomNum - 2).
        /// ex) Routes in 2nd floor are stored in index (2 - 1) - 1 = 0.
        /// </summary>
        public List<StairRouteContainer> StairToStairRoutes
        {
            get
            {
                return stairToStairRoutes;
            }
        }

        [SerializeField]
        private List<Transform> thiefGenertaionPoints;
        /// <summary>
        /// The points which can be selected as thief players' initial loacation.
        /// </summary>
        public List<Transform> ThiefGenertaionPoints
        {
            get
            {
                return thiefGenertaionPoints;
            }
        }

        //이슈: 도둑 생성 포인트의 층을 수동으로 유지중, 반드시 수정 필요.
        [SerializeField]
        private List<int> thiefGenerationPointsFloor;
        public List<int> ThiefGenerationPointsFloor
        {
            get
            {
                return thiefGenerationPointsFloor;
            }
        }

        [SerializeField]
        private List<Transform> detectiveGenerationPoints;
        /// <summary>
        /// The points which can be selected as detective players' initial loacation.
        /// </summary>
        public List<Transform> DetectiveGenerationPoints
        {
            get
            {
                return detectiveGenerationPoints;
            }
        }

        #endregion

        [SerializeField]
        private int maxRandomValue;
        public int MaxRandomValue
        {
            get
            {
                return maxRandomValue;
            }
        }

        private void Awake()
        {
            maxRandomValue = rooms.Count - 1;
            foreach (StairRouteContainer container in stairToStairRoutes)
            {
                if (container.LeftDownRoutes.Length != 0
                    && maxRandomValue % container.LeftDownRoutes.Length != 0)
                    maxRandomValue *= container.LeftDownRoutes.Length;
                if (container.LeftUpRoutes.Length != 0
                    && maxRandomValue % container.LeftUpRoutes.Length != 0)
                    maxRandomValue *= container.LeftUpRoutes.Length;
                if (container.RightDownRoutes.Length != 0
                    && maxRandomValue % container.RightDownRoutes.Length != 0)
                    maxRandomValue *= container.RightDownRoutes.Length;
                if (container.RightUpRoutes.Length != 0
                    && maxRandomValue % container.RightUpRoutes.Length != 0)
                    maxRandomValue *= container.RightUpRoutes.Length;
            }

            List<Route> NPCGenRoutes = new List<Route>();
            nPCGenPoints = new List<RouteNode>();

            int genPointCnt = 0;
            foreach (ExhibitRoom room in rooms)
            {
                //Find all NPC-generatable routes and make all routes set.
                NPCGenRoutes.Add(room.InRoomRoute);

                foreach (List<Route> routes in room.ToRoomRoutes.Values)
                {
                    NPCGenRoutes.AddRange(routes);

                    if (maxRandomValue % routes.Count != 0)
                        maxRandomValue *= routes.Count;
                }

                NPCGenRoutes.AddRange(room.FromStairRoutes.LeftDownRoutes);
                NPCGenRoutes.AddRange(room.FromStairRoutes.LeftUpRoutes);
                NPCGenRoutes.AddRange(room.FromStairRoutes.RightDownRoutes);
                NPCGenRoutes.AddRange(room.FromStairRoutes.RightUpRoutes);

                if (room.FromStairRoutes.LeftDownRoutes.Length != 0
                    && maxRandomValue % room.FromStairRoutes.LeftDownRoutes.Length != 0)
                    maxRandomValue *= room.FromStairRoutes.LeftDownRoutes.Length;
                if (room.FromStairRoutes.LeftUpRoutes.Length != 0
                    && maxRandomValue % room.FromStairRoutes.LeftUpRoutes.Length != 0)
                    maxRandomValue *= room.FromStairRoutes.LeftUpRoutes.Length;
                if (room.FromStairRoutes.RightDownRoutes.Length != 0
                    && maxRandomValue % room.FromStairRoutes.RightDownRoutes.Length != 0)
                    maxRandomValue *= room.FromStairRoutes.RightDownRoutes.Length;
                if (room.FromStairRoutes.RightUpRoutes.Length != 0
                    && maxRandomValue % room.FromStairRoutes.RightUpRoutes.Length != 0)
                    maxRandomValue *= room.FromStairRoutes.RightUpRoutes.Length;

                if (room.ToStairRoutes.LeftDownRoutes.Length != 0
                    && maxRandomValue % room.ToStairRoutes.LeftDownRoutes.Length != 0)
                    maxRandomValue *= room.ToStairRoutes.LeftDownRoutes.Length;
                if (room.ToStairRoutes.LeftUpRoutes.Length != 0
                    && maxRandomValue % room.ToStairRoutes.LeftUpRoutes.Length != 0)
                    maxRandomValue *= room.ToStairRoutes.LeftUpRoutes.Length;
                if (room.ToStairRoutes.RightDownRoutes.Length != 0
                    && maxRandomValue % room.ToStairRoutes.RightDownRoutes.Length != 0)
                    maxRandomValue *= room.ToStairRoutes.RightDownRoutes.Length;
                if (room.ToStairRoutes.RightUpRoutes.Length != 0
                    && maxRandomValue % room.ToStairRoutes.RightUpRoutes.Length != 0)
                    maxRandomValue *= room.ToStairRoutes.RightUpRoutes.Length;

                //Find all Item Generation Points
                foreach (ItemGenPoint itemGenPoint in room.ItemGenPoints)
                {
                    itemGenPoint.Index = genPointCnt++;
                    itemGenPoints.Add(itemGenPoint);
                }
            }

            List<Vector3> assignedLoc = new List<Vector3>();
            foreach (Route route in NPCGenRoutes)
            {
                for (int i = 1; i < route.NodeSet.Length - 1; i++)
                {
                    Vector3 nodeLoc = route.NodeSet[i].DefaultPos;
                    if (!assignedLoc.Contains(nodeLoc))
                    {
                        nPCGenPoints.Add(route.NodeSet[i]);
                        assignedLoc.Add(nodeLoc);
                    }
                }
            }

            NPCGenPointSelector = new int[nPCGenPoints.Count];
            for (int i = 0; i < nPCGenPoints.Count; i++)
                NPCGenPointSelector[i] = i;

            GlobalFunctions.RandomizeArray<int>(NPCGenPointSelector);

            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");
        }

        int[] NPCGenPointSelector;
        /*/// <summary>
        /// Check assigned NPC generation points. If assigned, true, if not false.
        /// </summary>
        bool[] ifPointAssigned;*/
        int assignedPointNum = 0;
        /// <summary>
        /// Return random NPC Generation Point(RouteNode).
        /// </summary>
        /// <returns>A random RouteNode that can be selected as NPC Generation Point. If all points are assigned, -1.</returns>
        public int GetRandomNPCGenPoint()
        {
            if (assignedPointNum >= nPCGenPoints.Count)  //All available points are assinged
                return -1;

            int ret = NPCGenPointSelector[assignedPointNum];
            assignedPointNum += 1;

            return ret;
        }

        public void InitNPCGenPoint()
        {
            assignedPointNum = 0;
            GlobalFunctions.RandomizeArray<int>(NPCGenPointSelector);
        }
    }
}
