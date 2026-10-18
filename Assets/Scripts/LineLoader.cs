using UnityEngine;
using UnityEngine.Assertions;

public class LineLoader : MonoBehaviour
{
    [SerializeField] private GameData _gameData;

    [SerializeField] private string _jsonToLoad;
    [SerializeField] private LineRenderer _lineRenderer;

    private Camera _camera;
    private LineSerializer _lineSerializer;

    private ScreenData _screenData;

    private void Awake()
    {
        _camera = Camera.main;
        _lineSerializer = new LineSerializer();

        _screenData = _gameData.DefaultScreenData;
    }

    [ContextMenu("Load Line")]
    private void LoadLine()
    {
        Assert.IsFalse(string.IsNullOrEmpty(_jsonToLoad));

        var lineData = _lineSerializer.FromJson(_jsonToLoad);
        _lineSerializer.BuildLine(lineData, _camera, _lineRenderer, _screenData);
    }

    [ContextMenu("Load Points")]
    private void LoadPoints()
    {
        Assert.IsFalse(string.IsNullOrEmpty(_jsonToLoad));

        var lineData = _lineSerializer.FromJson(_jsonToLoad);
        foreach (var lineDataPoint in lineData.Points)
        {
            var position = _lineSerializer.PointToWorld(lineDataPoint, _camera, _screenData);
            Instantiate(_gameData.PointPrefab, position, Quaternion.identity);
        }
    }

    [ContextMenu("PrintJson")]
    private void PrintJson()
    {
        Assert.IsNotNull(_lineRenderer);

        var lineData = _lineSerializer.GetLineData(_lineRenderer, _camera, _screenData);
        var json = _lineSerializer.ToJson(lineData);
        _jsonToLoad = json;
        Debug.Log(json);
    }
}
