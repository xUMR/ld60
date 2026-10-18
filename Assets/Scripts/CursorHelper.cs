using TMPro;
using UnityEngine;

public sealed class CursorHelper : MonoBehaviour
{
    [SerializeField] private Transform _cursorTransform;
    [SerializeField] private GameData _gameData;
    [SerializeField] private Camera _camera;
    [SerializeField] private TMP_Text _text;

    private LineSerializer _lineSerializer;
    private ScreenData _screenData;

#if UNITY_EDITOR
    private void Awake()
    {
        _lineSerializer = new LineSerializer();
        _screenData = _gameData.DefaultScreenData;
    }

    private void LateUpdate()
    {
        var point = _lineSerializer.WorldToPoint(_cursorTransform.position, _camera, _screenData);
        _text.text = point.ToString();
    }
#endif
}
