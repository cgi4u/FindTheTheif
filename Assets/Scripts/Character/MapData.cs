using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//NPC Routing 고려
//1. 기본적으로 Route를 이루는 Point들에게 다음 Point를 받아 이동한다. > 이 방법이면 Point의 중복이 전혀 없어야 한다. case 분리로 할수는 있겠지만...
//2, 대기가 있는 Point의 경우 대기 시간을 1~10초간 갖고 다음 Point로 이동한다.
//3. Weighted Random으로 짧은 시간 대기가 더 많이 나오게 한다.
//4. NPC끼리 부딫힐 경우에는 기다리면서 1초마다 Collision 체크를 시도해 나가면 내가 들어간다.
// 방 내부에서 이렇게 한다고 치고 한 방에서 다른 방으로 갈 때 랜덤을 적용하려면?
// 자연스럽게 이동하게 하려면 각 방 앞에 Point를 찍고, 계단을 포함한 각 Point끼리의 연결관계를 자료구조로 표현해서 PathFinding하도록 해야함.
// 그런데 이 자료구조는 맵에 고유하게 만들어줘야?
// 필요한 데이터들: 해당 포인트의 위치, 해당 포인트와 연결되어 있는 다음 포인터의 번호, 상하좌우
// 그냥 해당 point의 위치와, 인접배열정도 있으면 될듯
// 계단은 2줄로 하고, 올라갈때와 내려갈때 부딫히지 않도록
// 다른 층으로 가야할 때 이걸 어떻게 표현?
// 사실 이건 Map이 여러개라면 DB 형식이나 바이너리나 그렇게 표현해야되는데...

public static class MapData {

	public static int[,] pointAdjMatrix = { { 0, 1, 0, 1}};
}
