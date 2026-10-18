using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

public sealed class KeyboardMovement : MonoBehaviour
{
    [SerializeField] private GameData _gameData;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private Transform _target;
    [SerializeField] private float _throttle = 0.2f;
    [SerializeField] private float _throttleFast = 0.1f;

    private float _moveTimer;
    private Vector2 _input;
    private Camera _camera;
    private Transform _cameraTransform;
    private float _timerMin;

    private void Awake()
    {
        _camera = Camera.main;
        Assert.IsNotNull(_camera);

        _cameraTransform = _camera.transform;
        _timerMin = -_throttle / 2;
    }

    private void Update()
    {
        _speed = _camera.orthographicSize / _gameData.Resolution * 2;
        _input = GetWasdInput();

        if (_input == Vector2.zero)
        {
            _moveTimer = 0;
        }
        else
        {
            _moveTimer = Mathf.Max(_timerMin, _moveTimer - Time.deltaTime);
            if (_moveTimer <= 0)
            {
                MoveTargetTopDown(_target, _input);
                _moveTimer += _throttle;

                Debug.Log($"Moved at {Time.realtimeSinceStartupAsDouble}");
            }
        }

        if (Keyboard.current.cKey.wasPressedThisFrame || Keyboard.current.rKey.wasPressedThisFrame)
        {
            _cameraTransform.position = new Vector3(0, 0, -10);
        }
    }

    private void MoveTargetTopDown(Transform target, Vector2 input)
    {
        target.Translate(_speed * input);
    }

    private void MoveTargetSideScroll(Transform target, Vector2 input)
    {
        target.Translate(_speed * input);
    }

    private Vector2 GetWasdInput()
    {
        var x =
            Keyboard.current.dKey.isPressed ? 1 :
            Keyboard.current.aKey.isPressed ? -1 : 0;

        if (Mathf.Approximately(x, 0f))
        {
            x =
                Keyboard.current.rightArrowKey.isPressed ? 1 :
                Keyboard.current.leftArrowKey.isPressed ? -1 : 0;
        }

        var y =
            Keyboard.current.wKey.isPressed ? 1 :
            Keyboard.current.sKey.isPressed ? -1 : 0;

        if (Mathf.Approximately(y, 0f))
        {
            y =
                Keyboard.current.upArrowKey.isPressed ? 1 :
                Keyboard.current.downArrowKey.isPressed ? -1 : 0;
        }

        return new Vector2(x, y);
    }
}
