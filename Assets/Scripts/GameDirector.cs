using UnityEngine;
using UnityEngine.Assertions;

public class GameDirector : MonoBehaviour
{
    private static readonly string PlayerDataKey = "playerData";

    [SerializeField] private LevelLoader _levelLoader;
    [SerializeField] private MouseDrawer _mouseDrawer;
    [SerializeField] private TextDrawer _textDrawer;

    private PlayerData _playerData;

    private void Start()
    {
        _playerData = TryLoadPlayerData();

        GameEvents.PrevLevelRequested += HandlePrevLevelRequested;
        GameEvents.NextLevelRequested += HandleNextLevelRequested;

        GameEvents.LevelCheckRequested += HandleLevelCheckRequested;
        GameEvents.PlayerDataDeleteRequested += HandlePlayerDataDeleteRequested;
    }

    private PlayerData TryLoadPlayerData()
    {
        var json = PlayerPrefs.GetString(PlayerDataKey);
        if (string.IsNullOrEmpty(json))
            return new PlayerData();

        var playerData = JsonUtility.FromJson<PlayerData>(json);
        return playerData ?? new PlayerData();
    }

    private void OnDestroy()
    {
        GameEvents.PrevLevelRequested -= HandlePrevLevelRequested;
        GameEvents.NextLevelRequested -= HandleNextLevelRequested;

        GameEvents.LevelCheckRequested -= HandleLevelCheckRequested;
        GameEvents.PlayerDataDeleteRequested -= HandlePlayerDataDeleteRequested;
    }

    private void HandlePlayerDataDeleteRequested()
    {
        DeletePlayerData();
    }

    private void HandlePrevLevelRequested()
    {
        ClearLevel();
        _levelLoader.LoadPrevLevel();
    }

    private void HandleNextLevelRequested()
    {
        ClearLevel();
        _levelLoader.LoadNextLevel();
    }

    private void HandleLevelCheckRequested()
    {
        var result = _levelLoader.Check();
        if (result)
        {
            var isLevelClearedFirstTime = !_playerData.HasCompletedLevel(_levelLoader.CurrentLevel);

            _playerData.Progress |= 1 << _levelLoader.CurrentLevel;
            UpdatePlayerData(_playerData);

            CongratulatePlayerThenLoadNextLevelAsync(isLevelClearedFirstTime).LogExceptionsAndForget();
        }
    }

    private async Awaitable CongratulatePlayerThenLoadNextLevelAsync(bool isLevelClearedFirstTime)
    {
        GameEvents.Invoke.InputStateChangeRequested(false);

        var message = isLevelClearedFirstTime ? "Well done!" : "Well done... Again.";
        await _textDrawer.PrintTextAsync(message);
        await Awaitable.WaitForSecondsAsync(0.4f);
        await _textDrawer.PrintTextAsync("Loading next...");
        await Awaitable.WaitForSecondsAsync(0.4f);

        GameEvents.Invoke.NextLevelRequested();

        await _textDrawer.PrintTextAsync("This one has the label:\n" +
                                         $"{_levelLoader.CurrentLevelData.Hint}\n\n" +
                                         "Initializing...");
        await Awaitable.WaitForSecondsAsync(0.8f);

        _textDrawer.ClearText();
        GameEvents.Invoke.InputStateChangeRequested(true);
    }

    private void ClearLevel()
    {
        _mouseDrawer.Clear();
    }

    private void UpdatePlayerData(PlayerData playerData)
    {
        Assert.IsNotNull(playerData);

        var json = JsonUtility.ToJson(playerData);
        PlayerPrefs.SetString(PlayerDataKey, json);

#if UNITY_EDITOR
        Debug.Log(json);

        System.Text.StringBuilder stringBuilder = new();
        for (var i = 0; i <= 11; i++)
        {
            if (playerData.HasCompletedLevel(i))
            {
                stringBuilder.Append($"{i} ");
            }
        }

        Debug.Log($"Completed levels: {stringBuilder}");
#endif
    }

    [ContextMenu("Clear Player Data")]
    private void DeletePlayerData()
    {
        PlayerPrefs.DeleteKey(PlayerDataKey);
        _playerData = new PlayerData();
    }
}
