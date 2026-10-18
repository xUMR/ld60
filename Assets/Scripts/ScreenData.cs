using UnityEngine;

public sealed class ScreenData
{
    public readonly Vector2 ClampMin;
    public readonly Vector2 ClampMax;
    public readonly float Unit;

    public ScreenData(Vector2 clampMin, Vector2 clampMax, float unit)
    {
        ClampMin = clampMin;
        ClampMax = clampMax;
        Unit = unit;
    }
}
