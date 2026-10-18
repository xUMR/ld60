using UnityEngine;
using UnityEngine.Assertions;

public class GameDirector : MonoBehaviour
{
    private static readonly string PlayerDataKey = "playerData";

    [SerializeField] private LevelCollection _levelCollection;
    [SerializeField] private LevelLoader _levelLoader;
    [SerializeField] private MouseDrawer _mouseDrawer;
    [SerializeField] private TextDrawer _textDrawer;

    private PlayerData _playerData;

    private void Start()
    {
        _playerData = TryLoadPlayerData();

        GameEvents.PrevLevelRequested += HandlePrevLevelRequested;
        GameEvents.NextLevelRequested += HandleNextLevelRequested;
        GameEvents.LevelRequested += HandleLevelRequested;

        GameEvents.LevelCheckRequested += HandleLevelCheckRequested;
        GameEvents.PlayerDataDeleteRequested += HandlePlayerDataDeleteRequested;

        GameEvents.AnyKeyPressed += HandleAnyKeyPressed;

        PlayInitializationSequenceAsync().LogExceptionsAndForget();
    }

    private void OnDestroy()
    {
        GameEvents.PrevLevelRequested -= HandlePrevLevelRequested;
        GameEvents.NextLevelRequested -= HandleNextLevelRequested;
        GameEvents.LevelRequested -= HandleLevelRequested;

        GameEvents.LevelCheckRequested -= HandleLevelCheckRequested;
        GameEvents.PlayerDataDeleteRequested -= HandlePlayerDataDeleteRequested;

        GameEvents.AnyKeyPressed -= HandleAnyKeyPressed;
    }

    private async Awaitable PlayInitializationSequenceAsync()
    {
        GameEvents.Invoke.InputStateChangeRequested(InputState.Disabled);
        if (_playerData.Progress == 0)
        {
            await _textDrawer.PrintTextAsync(
                "Welcome to LDR -\n" +
                "Linear Data Recovery\n\n" +
                "Your first task is ready.");
            await Awaitable.WaitForSecondsAsync(1.2f);

            GameEvents.Invoke.NextLevelRequested();
        }
        else
        {
            await _textDrawer.PrintTextAsync(
                "Welcome back to LDR -\n" +
                "Linear Data Recovery\n\n" +
                "Your next task is ready.");
            await Awaitable.WaitForSecondsAsync(1.2f);

            GameEvents.Invoke.LevelRequested(_playerData.FirstIncompleteLevel);
        }

        GameEvents.Invoke.InputStateChangeRequested(InputState.AnyKeyToProceed);
        await _textDrawer.PrintTextAsync(
            "Recover:\n" +
            $"{_levelLoader.CurrentLevelData.Hint}\n\n"+
            "Press Enter when the task\n" +
            "is complete.\n\n\n\n\n\n" +
            "Press any key to continue.");
    }

    private void HandleAnyKeyPressed()
    {
        if (_textDrawer.IsBusy)
        {
            _textDrawer.ProceedReveal();
        }
        else
        {
            _textDrawer.ClearText();
            GameEvents.Invoke.InputStateChangeRequested(InputState.Enabled);
        }
    }

    private PlayerData TryLoadPlayerData()
    {
        var json = PlayerPrefs.GetString(PlayerDataKey);
        if (string.IsNullOrEmpty(json))
            return new PlayerData();

        var playerData = JsonUtility.FromJson<PlayerData>(json);
        return playerData ?? new PlayerData();
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

    private void HandleLevelRequested(int level)
    {
        ClearLevel();
        _levelLoader.LoadLevel(level);
    }

    private void HandleLevelCheckRequested()
    {
        var result = _levelLoader.Check();
        if (result)
        {
            var isLevelClearedFirstTime = !_playerData.HasCompletedLevel(_levelLoader.CurrentLevel);

            if (_levelCollection.LevelAssets.Length == _levelLoader.CurrentLevel + 1)
            {
                _playerData.HasCompletedAllLevels = true;
            }

            _playerData.Progress |= 1 << _levelLoader.CurrentLevel;
            UpdatePlayerData(_playerData);

            if (_playerData.HasCompletedAllLevels)
            {
                CongratulateGameOverAsync().LogExceptionsAndForget();
            }
            else
            {
                CongratulatePlayerThenLoadNextLevelAsync(isLevelClearedFirstTime).LogExceptionsAndForget();
            }
        }
    }

    private async Awaitable CongratulateGameOverAsync()
    {
        GameEvents.Invoke.InputStateChangeRequested(InputState.Disabled);

        await _textDrawer.PrintTextAsync(
            "You managed to recover all the items.\n" +
            "Thank you for your effort.\n");

        await Awaitable.WaitForSecondsAsync(1.2f);

        _textDrawer.ClearText();
        GameEvents.Invoke.InputStateChangeRequested(InputState.Enabled);
    }

    private async Awaitable CongratulatePlayerThenLoadNextLevelAsync(bool isLevelClearedFirstTime)
    {
        GameEvents.Invoke.InputStateChangeRequested(InputState.Disabled);

        var message = isLevelClearedFirstTime
            ? "Well done!\nLoading next task..."
            : "Well done... Again?\nLoading next task...";

        await _textDrawer.PrintTextAsync(message);
        await Awaitable.WaitForSecondsAsync(0.4f);

        GameEvents.Invoke.NextLevelRequested();
        GameEvents.Invoke.InputStateChangeRequested(InputState.AnyKeyToProceed);

        await _textDrawer.PrintTextAsync(
            "Recover:\n" +
            $"{_levelLoader.CurrentLevelData.Hint}\n\n\n\n\n\n\n\n\n" +
            "Press any key to continue.");
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

public enum InputState
{
    Disabled,
    AnyKeyToProceed,
    Enabled
}
