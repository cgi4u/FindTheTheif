using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class TestNPCGenerator : MonoBehaviour
    {
        public GameObject NPCPrefab;

        void Start()
        {
            //Route Test
            for (int i = 0; i < 10; i++)
            {
                Debug.Log("Random route " + i);

                Route randomRoute = RoutingManager.Instance.getRandomRoute();
                switch (randomRoute.routeType)
                {
                    case Route.RouteType.In_Room:
                        Debug.Log("In-Room Route");
                        Debug.Log("Room num: " + randomRoute.curRoom);
                        break;
                    case Route.RouteType.Room_to_Room:
                        Debug.Log("Room-to-Room Route");
                        Debug.Log("Start Room num: " + randomRoute.startRoom);
                        Debug.Log("End Room num: " + randomRoute.endRoom);
                        break;
                    case Route.RouteType.Stair_to_Room:
                        Debug.Log("Stair-to-Room Route");
                        Debug.Log("Room num: " + randomRoute.endRoom);
                        Debug.Log("Stair type: " + randomRoute.stairType);
                        Debug.Log("Stair side: " + randomRoute.stairSide);
                        break;
                    case Route.RouteType.Stair_to_Stair:
                    case Route.RouteType.Room_to_Stair:
                        Debug.Log("to-Stair route: Error, should not be seleted.");
                        break;
                }

                GameObject newNPC = Instantiate(NPCPrefab);
                newNPC.GetComponent<NPCController>().ManualStart(randomRoute);
            }
        }
    }
}
