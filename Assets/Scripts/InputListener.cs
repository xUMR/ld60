using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputListener : MonoBehaviour
{
    public bool InputEnabled { get; private set; } = true;

    private void Start()
    {
        GameEvents.InputStateChangeRequested += HandleInputStateChangeRequested;
    }

    private void OnDestroy()
    {
        GameEvents.InputStateChangeRequested -= HandleInputStateChangeRequested;
    }

    private void HandleInputStateChangeRequested(bool isEnabled)
    {
        InputEnabled = isEnabled;
    }

    // todo use input system instead of polling for input
    private void Update()
    {
        if (!InputEnabled)
            return;

        var keyboard = Keyboard.current;

        if (keyboard.bKey.wasPressedThisFrame)
        {
            // load previous level
            GameEvents.Invoke.PrevLevelRequested();
        }

        if (keyboard.nKey.wasPressedThisFrame)
        {
            // load next level
            GameEvents.Invoke.NextLevelRequested();
        }

        if (keyboard.zKey.wasPressedThisFrame)
        {
            // undo last move
        }

        if (keyboard.rKey.wasPressedThisFrame)
        {
            // clear drawn lines
        }
#if UNITY_EDITOR
        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            // for debugging
        }
#endif

        if (keyboard.enterKey.wasPressedThisFrame)
        {
            // check level end
            GameEvents.Invoke.LevelCheckRequested();
        }

        if (keyboard.ctrlKey.isPressed && keyboard.shiftKey.isPressed && keyboard.xKey.wasPressedThisFrame)
        {
            GameEvents.Invoke.PlayerDataDeleteRequested();
        }

        var mouse = Mouse.current;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // temp line set start position
        }

        if (Mouse.current.leftButton.isPressed)
        {
            // temp line set end position
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            // check if valid line
        }
    }
}
