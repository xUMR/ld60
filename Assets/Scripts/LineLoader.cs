using UnityEngine;
using UnityEngine.Assertions;

public class LineLoader : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private string _jsonToLoad;
    [SerializeField] private LineRenderer _lineRenderer;

    [SerializeField] private Transform _pointsParent;

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
        LoadLine(lineData);
    }

    public void LoadLine(LineData lineData)
    {
        Assert.IsNotNull(lineData);
        Assert.IsNotNull(lineData.Points);
        Assert.IsTrue(lineData.Points.Count > 0);

        _lineSerializer.BuildLine(lineData, _camera, _lineRenderer, _screenData);
    }

    [ContextMenu("Load Points")]
    private void LoadPoints()
    {
        Assert.IsFalse(string.IsNullOrEmpty(_jsonToLoad));

        var lineData = _lineSerializer.FromJson(_jsonToLoad);
        LoadPoints(lineData);
    }

    public void LoadPoints(LineData lineData, int count = 0)
    {
        var iterations = count <= 0
            ? lineData.Points.Count
            : Mathf.Min(count, lineData.Points.Count);

        for (var i = 0; i < iterations; i++)
        {
            var lineDataPoint = lineData.Points[i];
            var position = _lineSerializer.PointToWorld(lineDataPoint, _camera, _screenData);
            Instantiate(_gameData.PointPrefab, position, Quaternion.identity, new InstantiateParameters
            {
                parent = _pointsParent
            });
        }
    }

    public void ClearPoints()
    {
        for (var i = 0; i < _pointsParent.childCount; i++)
        {
            Destroy(_pointsParent.GetChild(i).gameObject);
        }
    }

    public void Clear()
    {
        ClearPoints();
        _lineRenderer.positionCount = 0;
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
