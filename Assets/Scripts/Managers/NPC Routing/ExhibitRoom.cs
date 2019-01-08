using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    // 각 전시실의 번호와 층을 저장
    // NPC가 이동시 계단을 경유해야 하는지 판단하기 위해 사용
    public class ExhibitRoom : MonoBehaviour
    {
        public int num;
        public int floor;
    }
}
