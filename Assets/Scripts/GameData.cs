using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "SO/GameData")]
public sealed class GameData : ScriptableObject
{
    [SerializeField] private int _resolution = 30;
    [SerializeField] private float _clamp = 0.1f;
    [SerializeField] private GameObject _pointPrefab;

    private ScreenData _defaultScreenData;
    private Camera _defaultCamera;
    private LineSerializer _defaultLineSerializer;

    public int Resolution => _resolution;

    public ScreenData DefaultScreenData => _defaultScreenData ??=
        new ScreenData(
            GetClampMin(Screen.width, Screen.height),
            GetClampMax(Screen.width, Screen.height),
            GetUnitHeight(Screen.height));

    public Camera DefaultCamera
    {
        get
        {
            // camera is a unity object so don't use ??= check
            if (_defaultCamera == null)
            {
                _defaultCamera = Camera.main;
            }
            return _defaultCamera;
        }
    }

    public LineSerializer DefaultLineSerializer => _defaultLineSerializer ??= new LineSerializer();

    public GameObject PointPrefab => _pointPrefab;

    public int GetUnitHeight(int height) => height / _resolution;

    public Vector2 GetClampMin(int width, int height) => new(width * _clamp, height * _clamp);
    public Vector2 GetClampMax(int width, int height) => new(width * (1 - _clamp), height * (1 - _clamp));
}
