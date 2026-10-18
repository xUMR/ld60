using System;

[Serializable]
public sealed class PlayerData
{
    public bool HasSeenTutorial;
    public int Progress; // 12 levels, 1 bit each, 20 bits unused

    public bool HasCompletedLevel(int level)
    {
        var mask = 1 << level;
        return (Progress & mask) == mask;
    }
}
