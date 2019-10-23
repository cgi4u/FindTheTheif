using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace com.MJT.FindTheThief
{
    public class NewRoute : MonoBehaviour
    {
        public enum ERouteType { ToRoom, InRoom, ToDownstair, ToUpstair }

        [SerializeField]
        private ERouteType routeType;
        public ERouteType RouteType => routeType;

        [SerializeField]
        private Transform[] nodes;
        public Transform[] Nodes => nodes;

        [Serializable]
        class RouteToOhterRoom
        {
            [SerializeField]
            List<NewRoute> subRoutes;
        }

        static readonly int roomCount = 3;
        [SerializeField]
        private List<RouteToOhterRoom> routes;

        private void Awake()
        {
            /* Initialize for weighted random */
            
        }


    }
}
