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

        #endregion

        private void Awake()
        {
            List<Route> NPCGenRoutes = new List<Route>();
            nPCGenPoints = new List<RouteNode>();

            int genPointCnt = 0;
            foreach (ExhibitRoom room in rooms)
            {
                //Find all NPC-generatable routes
                NPCGenRoutes.Add(room.InRoomRoute);
                foreach (List<Route> routes in room.ToRoomRoutes.Values)
                {
                    NPCGenRoutes.AddRange(routes);
                }
                if (room.FromStairRoutes.LeftDownRoute != null)
                    NPCGenRoutes.Add(room.FromStairRoutes.LeftDownRoute);
                if (room.FromStairRoutes.LeftUpRoute != null)
                    NPCGenRoutes.Add(room.FromStairRoutes.LeftUpRoute);
                if (room.FromStairRoutes.RightDownRoute != null)
                    NPCGenRoutes.Add(room.FromStairRoutes.RightDownRoute);
                if (room.FromStairRoutes.RightUpRoute != null)
                    NPCGenRoutes.Add(room.FromStairRoutes.RightUpRoute);

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

            /*//ifRouteAssigned = new bool[allGenerationRoutes.Count];
            ifPointAssigned = new bool[nPCGenPoints.Count];*/

            NPCGenPointSelector = new int[nPCGenPoints.Count];
            for (int i = 0; i < nPCGenPoints.Count; i++)
                NPCGenPointSelector[i] = i;

            Globals.RandomizeArray<int>(NPCGenPointSelector);

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
            Globals.RandomizeArray<int>(NPCGenPointSelector);
        }
    }
}
