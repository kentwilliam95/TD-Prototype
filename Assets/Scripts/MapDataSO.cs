using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDataSO : ScriptableObject
{
    [TextArea(10, 30)]
    public string mapData;

    public int _sizeX;
    public int _sizeY;
}
