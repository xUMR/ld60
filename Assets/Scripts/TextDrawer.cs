using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class TextDrawer : MonoBehaviour
{
    [SerializeField] private TMP_Text _tmpText;
    [SerializeField] private GameObject _textFrame;
    [SerializeField] private GameObject _textBackground;
    [SerializeField] private float _revealDurationPerChar;
    [SerializeField] private string _revealTextDebug;

    private CancellationTokenSource _cts;

    public bool IsBusy { get;  private set; }

    private void OnDestroy()
    {
        _cts?.Dispose();
    }

    public void ClearText()
    {
        CancelCurrentRevealTask();
        _tmpText.text = "";
        ToggleObjects(false);
    }

    private void ToggleObjects(bool state)
    {
        // _textFrame.SetActive(state);
        _textBackground.SetActive(state);
        _tmpText.gameObject.SetActive(state);
    }

    // public void PrintText(string text, bool revealOverTime = true)
    // {
    //     PrintTextAsync(text, revealOverTime).LogExceptionsAndForget();
    // }

    public async Awaitable PrintTextAsync(string text, bool revealOverTime = true)
    {
        CancelCurrentRevealTask();
        _tmpText.text = text;
        ToggleObjects(true);

        if (revealOverTime)
        {
            var totalDuration = _tmpText.text.Length * _revealDurationPerChar;
            _cts = new CancellationTokenSource();
            await RevealTextAsync(totalDuration);
        }
    }

    // [ContextMenu("Reveal")]
    // public void Reveal()
    // {
    //     var totalDuration = _tmpText.text.Length * _revealDurationPerChar;
    //
    //     CancelCurrentRevealTask();
    //     RevealTextAsync(totalDuration);
    // }

    private void CancelCurrentRevealTask()
    {
        _cts?.Cancel();
        IsBusy = false;
    }

    private async Awaitable RevealTextAsync(float duration = 0)
    {
        IsBusy = true;
        _tmpText.maxVisibleCharacters = 0;

        if (!string.IsNullOrEmpty(_revealTextDebug))
        {
            _tmpText.text = _revealTextDebug;
        }

        try
        {
            var textLength = _tmpText.text.Length;
            duration = duration > 0 ? duration : textLength * _revealDurationPerChar;
            for (var t = 0f; t < 1; t += Time.deltaTime / duration)
            {
                var visibleCharacterCount = Mathf.RoundToInt(Mathf.Lerp(0, textLength, t));
                var isVisibleCharacterRevealed =
                    visibleCharacterCount != _tmpText.maxVisibleCharacters &&
                    !char.IsWhiteSpace(_tmpText.text[visibleCharacterCount - 1]);

                if (isVisibleCharacterRevealed)
                {
                    GameEvents.Invoke.ClickRequested();
                }

                _tmpText.maxVisibleCharacters = visibleCharacterCount;

                await Awaitable.NextFrameAsync(_cts.Token);
            }
        }
        catch (OperationCanceledException) { } // expected
        finally
        {
            _tmpText.maxVisibleCharacters = _tmpText.text.Length;
            IsBusy = false;
        }
    }

    public void ProceedReveal()
    {
        CancelCurrentRevealTask();
        _tmpText.maxVisibleCharacters = _tmpText.text.Length;
    }
}
