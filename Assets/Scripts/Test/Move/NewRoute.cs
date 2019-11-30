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
        private List<NewSubroute> subRoutes;
        public List<NewSubroute> SubRoutes => subRoutes;

        [SerializeField]
        private List<NewRoute> nextRoutes;
        public List<NewRoute> NextRoutes => nextRoutes;
    }
}
