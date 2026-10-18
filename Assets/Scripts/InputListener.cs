using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputListener : MonoBehaviour
{
    public bool InputEnabled = true;

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

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            // for debugging
        }

        if (keyboard.enterKey.wasPressedThisFrame)
        {
            // check level end
            GameEvents.Invoke.LevelCheckRequested();
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
