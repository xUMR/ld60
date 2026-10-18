using UnityEngine;

public class GameDirector : MonoBehaviour
{
    [SerializeField] private LevelLoader _levelLoader;
    [SerializeField] private MouseDrawer _mouseDrawer;

    private void Start()
    {
        GameEvents.PrevLevelRequested += HandlePrevLevelRequested;
        GameEvents.NextLevelRequested += HandleNextLevelRequested;

        GameEvents.LevelCheckRequested += HandleLevelCheckRequested;
    }

    private void OnDestroy()
    {
        GameEvents.PrevLevelRequested -= HandlePrevLevelRequested;
        GameEvents.NextLevelRequested -= HandleNextLevelRequested;

        GameEvents.LevelCheckRequested -= HandleLevelCheckRequested;
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

    private void ClearLevel()
    {
        _mouseDrawer.Clear();
    }

    private void HandleLevelCheckRequested()
    {
        var result = _levelLoader.Check();
        if (result)
        {
            // congratulate the player
            // then load next level

            GameEvents.Invoke.NextLevelRequested();
        }
    }
}
