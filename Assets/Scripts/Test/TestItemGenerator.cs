using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace com.MJT.FindTheTheif
{
    public class TestItemGenerator : MonoBehaviour
    {
        public readonly int itemNum = 10;
        public GameObject[] itemPrefabs;

        private void Awake()
        {
        }

        // Start is called before the first frame update
        void Start()
        {
            while (MapDataManager.Instance == null) ;
            RandomItemGeneration();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void RandomItemGeneration()
        {
            for (int i = 0; i < itemNum; i++)
            {
                int r1 = Random.Range(0, itemNum);
                int r2 = Random.Range(0, itemNum);

                GameObject temp = itemPrefabs[r1];
                itemPrefabs[r1] = itemPrefabs[r2];
                itemPrefabs[r2] = temp;
            }

            MapDataManager mapDataManager = MapDataManager.Instance;
            if (mapDataManager.ItemGenerationPoints.Count > itemNum)
            {
                Debug.LogError("There are fewer items than generation points.");
                return;
            }

            for (int i = 0; i < mapDataManager.ItemGenerationPoints.Count; i++)
            {
                GameObject newItem = Instantiate(itemPrefabs[i], mapDataManager.ItemGenerationPoints[i].position, Quaternion.identity);

                ExhibitRoom roomOfItem = mapDataManager.ItemGenerationPoints[i].GetComponentInParent<ExhibitRoom>();
                newItem.GetComponent<ItemController>().Init(roomOfItem.floor, roomOfItem.num);
            }
        }
    }
}
