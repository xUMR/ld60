using System;
using UnityEngine;

public sealed class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private AudioData _lineStarted;
    [SerializeField] private AudioData _lineCompleted;
    [SerializeField] private AudioData _lineFailed;
    [SerializeField] private AudioData _lineUndo;

    [SerializeField] private AudioData _levelCompleted;
    [SerializeField] private AudioData _levelCompleteFailed;

    [SerializeField] private AudioData _click;

    private void Start()
    {
        GameEvents.LineStarted += PlayLineStarted;
        GameEvents.LineCompleted += PlayLineCompleted;
        GameEvents.LineUndoPerformed += PlayLineUndoPerformed;
        GameEvents.LevelCompleted += PlayLevelCompleted;
        GameEvents.ClickRequested += PlayClickRequested;
    }

    private void OnDestroy()
    {
        GameEvents.LineStarted -= PlayLineStarted;
        GameEvents.LineCompleted -= PlayLineCompleted;
        GameEvents.LineUndoPerformed -= PlayLineUndoPerformed;
        GameEvents.LevelCompleted -= PlayLevelCompleted;
        GameEvents.ClickRequested -= PlayClickRequested;
    }

    private void PlayLineStarted() => Play(_lineStarted);
    private void PlayLineCompleted(bool result) => Play(result ? _lineCompleted : _lineFailed);
    private void PlayLineUndoPerformed() => Play(_lineUndo);
    private void PlayLevelCompleted(bool result) => Play(result ? _levelCompleted : _levelCompleteFailed);
    private void PlayClickRequested() => Play(_click);

    private void Play(AudioData audioData)
    {
        audioData.Apply(_audioSource);
        _audioSource.Play();
    }
}
