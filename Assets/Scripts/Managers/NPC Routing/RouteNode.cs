using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // 각 노드의 데이터를 저장하는 객체
    public class RouteNode : MonoBehaviour
    {
        // 아이템 관람 지점인지, 맞다면 아이템의 방향은 어느쪽인지 판단
        public bool ifItemPoint;
        public Direction itemDir;
    }
}
