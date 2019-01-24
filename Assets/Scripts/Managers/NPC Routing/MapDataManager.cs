using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class FloorSTSRouteContainer
    {
        public Route leftDownDownRoute;
        public Route leftUpUpRoute;
        public Route rightDownDownRoute;
        public Route rightUpUpRoute;
    }

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
        private ExhibitRoom[] rooms;
        /// <summary>
        /// Room datas(should be serialized)
        /// </summary>
        public ExhibitRoom[] Rooms
        {
            get
            {
                return rooms;
            }
        }

        private List<Transform> itemGenPoints = new List<Transform>();
        /// <summary>
        /// Points where items can be generated
        /// </summary>
        public List<Transform> ItemGenPoints
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
        private FloorSTSRouteContainer[] stairToStairRoutes;
        public FloorSTSRouteContainer[] StairToStairRoutes
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

            foreach (ExhibitRoom room in rooms)
            {
                NPCGenRoutes.Add(room.InRoomRoute);
                NPCGenRoutes.AddRange(room.ToRoomRoutes);
                foreach (Route fromStairRoute in room.FromStairRoutes)
                {
                    if (fromStairRoute != null)
                        NPCGenRoutes.Add(fromStairRoute);
                }

                itemGenPoints.AddRange(room.ItemGenPoints);
            }

            List<Vector3> assignedLoc = new List<Vector3>();
            foreach (Route route in NPCGenRoutes)
            {
                for (int i = 1; i < route.NodeSet.Length - 1; i++)
                {
                    Vector3 nodeLoc = route.NodeSet[i].transform.position;
                    if (assignedLoc.FindAll(loc => loc.Equals(nodeLoc)).Count == 0)
                    {
                        nPCGenPoints.Add(route.NodeSet[i]);
                        assignedLoc.Add(nodeLoc);
                    }
                }
            }

            //ifRouteAssigned = new bool[allGenerationRoutes.Count];
            ifPointAssigned = new bool[nPCGenPoints.Count];

            //Routing Manager Singlton 생성
            if (instance == null)
            {
                instance = this;
            }
            else
                Debug.Log("Error: Multiple instantiation of the routing manager.");
        }

        private void Start()
        {
            
        }

        /*
        bool[] ifRouteAssigned;
        int assignedRouteNum = 0;
        public Route getRandomRoute()
        {
            if (assignedRouteNum >= allGenerationRoutes.Count)  //All available routes are assinged
                return null;                

            int r;
            do
            {
                r = Random.Range(0, allGenerationRoutes.Count);
            } while (ifRouteAssigned[r]);

            ifRouteAssigned[r] = true;
            assignedRouteNum += 1;

            return allGenerationRoutes[r];
        }
        */


        /// <summary>
        /// Check assigned NPC generation points. If assigned, true, if not false.
        /// </summary>
        bool[] ifPointAssigned;
        int assignedPointNum = 0;
        /// <summary>
        /// Return random NPC Generation Point(RouteNode).
        /// </summary>
        /// <returns>A random RouteNode that can be selected as NPC Generation Point. If all points are assigned, -1.</returns>
        public int GetRandomNPCGenPointIdx()
        {
            if (assignedPointNum >= nPCGenPoints.Count)  //All available points are assinged
                return -1;

            int r;
            do
            {
                r = Random.Range(0, nPCGenPoints.Count);
            } while (ifPointAssigned[r]);

            ifPointAssigned[r] = true;
            assignedPointNum += 1;

            return r;
        }
    }
}
