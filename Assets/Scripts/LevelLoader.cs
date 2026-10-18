using UnityEngine;

public class LevelLoader : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private LevelCollection _levelCollection;
    [SerializeField] private LineLoader _lineLoader;
    [SerializeField] private MouseDrawer _mouseDrawer;

    private LevelData _loadedLevelData;
    private int _loadedLevelIndex = -1;

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
        if (levelData.AuxLineData != null && levelData.AuxLineData.Points != null)
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
        var actualLineData = _gameData.DefaultLineSerializer.GetLineData(
            _mouseDrawer.MainRenderer,
            _gameData.DefaultCamera,
            _gameData.DefaultScreenData);

        var expectedLineData = _loadedLevelData.LineData;

        // held together by actual tape
        return actualLineData.Matches(expectedLineData) || expectedLineData.Matches(actualLineData);
    }
}
