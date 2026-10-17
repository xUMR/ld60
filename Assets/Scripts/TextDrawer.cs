using TMPro;
using UnityEngine;

public class TextDrawer : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private float _revealDurationPerChar;
    [SerializeField] private string _revealTextDebug;

    private Awaitable _currentRevealTask;

    public void PrintText(string text)
    {
        _tmpText.text = text;
    }

    [ContextMenu("Reveal")]
    public void Reveal()
    {
        var totalDuration = _tmpText.text.Length * _revealDurationPerChar;

        if (_currentRevealTask != null)
        {
            _currentRevealTask.Cancel();
            _currentRevealTask = null;
        }
        _currentRevealTask = RevealTextAsync(totalDuration);
    }

    private async Awaitable RevealTextAsync(float duration = 0)
    {
        _tmpText.maxVisibleCharacters = 0;

        if (!string.IsNullOrEmpty(_revealTextDebug))
        {
            _tmpText.text = _revealTextDebug;
        }

        var textLength = _tmpText.text.Length;
        duration = duration > 0 ? duration : textLength * _revealDurationPerChar;
        for (var t = 0f; t < 1; t += Time.deltaTime / duration)
        {
            var visibleCharacterCount = Mathf.RoundToInt(Mathf.Lerp(0, textLength, t));
            _tmpText.maxVisibleCharacters = visibleCharacterCount;

            await Awaitable.NextFrameAsync();
        }

        _tmpText.maxVisibleCharacters = _tmpText.text.Length;
    }
}
