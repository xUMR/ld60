using System;

[Serializable]
public sealed class PlayerData
{
    public bool HasCompletedAllLevels;
    public int Progress; // 12 levels, 1 bit each, 20 bits unused

    public bool HasCompletedLevel(int level)
    {
        var mask = 1 << level;
        return (Progress & mask) == mask;
    }

    public int FirstIncompleteLevel
    {
        get
        {
            for (var i = 0; i < 12; i++)
                if (!HasCompletedLevel(i))
                    return i;

            return 0;
        }
    }
}
