using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class LineData
{
    public bool IsLoop;
    public List<Vector2Int> Points;
}
