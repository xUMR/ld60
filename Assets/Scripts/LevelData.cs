using System;
using System.Diagnostics.CodeAnalysis;

[Serializable]
public sealed class LevelData
{
    // public int LevelNumber;
    public LineData LineData;
    [MaybeNull] public LineData AuxLineData;
    public string Hint;
}
