using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private LevelCollection _levelCollection;
    [SerializeField] private LineLoader _lineLoader;
    [SerializeField] private MouseDrawer _mouseDrawer;

    private LevelData _loadedLevelData;
    private int _loadedLevelIndex = -1;

    public int CurrentLevel => _loadedLevelIndex;

    public LevelData CurrentLevelData => _loadedLevelData;

    public void LoadPrevLevel() => LoadLevel(_loadedLevelIndex - 1);
    public void LoadNextLevel() => LoadLevel(_loadedLevelIndex + 1);

    public void LoadLevel(int level)
    {
        if (level >= _levelCollection.LevelAssets.Length || level < 0)
            return;

        Debug.Log($"Loading level {level}");
        var levelAsset = _levelCollection.LevelAssets[level];
        var levelData = JsonUtility.FromJson<LevelData>(levelAsset.text);

        _lineLoader.Clear();
        if (levelData.AuxLineData is { Points: { Count: > 0 } })
        {
            _lineLoader.LoadLine(levelData.AuxLineData);
        }
        _lineLoader.LoadPoints(levelData.LineData);

        _loadedLevelData = levelData;
        _loadedLevelIndex = level;
        Debug.Log($"Loaded level {level} ({levelData.Hint})");
    }

    public bool Check()
    {
        var actualLines = _mouseDrawer.LineSubSegmentSet;
        var expectedLines = _loadedLevelData.LineSegments;
        return actualLines.SetEquals(expectedLines);
    }
}
