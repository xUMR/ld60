using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

[Serializable]
public sealed class LevelData
{
    // public int LevelNumber;
    public List<LineSegment> LineSegments;
    public LineData LineData;
    [MaybeNull] public LineData AuxLineData;
    public string Hint;
}
