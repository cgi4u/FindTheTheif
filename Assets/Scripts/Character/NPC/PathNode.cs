using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Single, Multi };

public class PathNode: MonoBehaviour {
    public NodeType nodeType;
    public int roomNum; //If room-front node

    #region Single-way Node

    public string direction;

    #endregion


    #region Multi-way Node

    public List<int> upSelector;
    public List<int> downSelector;
    public List<int> rightSelector;
    public List<int> leftSelector;

    #endregion

}
