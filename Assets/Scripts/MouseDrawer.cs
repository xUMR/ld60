using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseDrawer : MonoBehaviour
{
    // python to check valid _resolution values
    // [x for x in range(16, 256) if h // x * x == h and w // (h // x) * (h // x) == w]
    // 960x540:   [18, 27, 36, 45, 54,     90, 108, 135,      180]
    // 1280x720:  [18,     36, 45,     72, 90,           144, 180]
    // 1920x1080: [18, 27, 36, 45, 54, 72, 90, 108, 135,      180, 216]
    // 2560x1440: [18,     36, 45,     72, 90,           144, 180]
    // 3840x2160: [18, 27, 36, 45, 54, 72, 90, 108, 135, 144, 180, 216]

    // 800x600:   [24, 30, 60, 75, 120, 150]

    [SerializeField] private int _resolution = 36; // vertical units
    private int _unitHeight;

    [SerializeField] private int _initialLineCount = 8;
    [SerializeField] private float _lineWidth = 0.01f;
    [SerializeField] private float _tolerance = 0.01f;

    // [SerializeField] private Vector2 _clamp = new Vector2(9.6f, 7.2f);
    [SerializeField] private GameData _gameData;
    // [SerializeField] private float _clamp = 0.1f;
    private Vector2 _clampMin;
    private Vector2 _clampMax;

    [SerializeField] private Material _lineMaterial;
    [SerializeField] private LineRenderer _tempLineRenderer;
    [SerializeField] private List<LineRenderer> _lineRenderers;
    [SerializeField] private Transform _cursor;

    // private List<Vector3[]> _linePoints;
    private int _rendererIndex;
    private Camera _camera;
    private ScreenData _screenData;
    // private UndoState _undoState;
    private static readonly int MaxUndoCount = 32;
    private UndoState[] _undoStates;
    private int _undoIndex;
    private int _undoCount; // remaining undo count

    // force single line
    private bool IsNewLineAllowed =>
        MainRenderer.positionCount == 2 && MainRenderer.GetPosition(0) == MainRenderer.GetPosition(1);

    public int Resolution => _resolution;

    public bool IsLineCursorVisible
    {
        get => _cursor.gameObject.activeSelf;
        set => _cursor.gameObject.SetActive(value);
    }

    public bool IsInputEnabled { get; set; } = true;

    public LineRenderer MainRenderer => _lineRenderers[0];

    private void Awake()
    {
        Cursor.visible = false;

        InitializeLineRenderers();

        _camera = Camera.main;
        _unitHeight = _gameData.GetUnitHeight(Screen.height);

        // clamp=0.1f -> screen [0.1-0.9] of the actual values
        _clampMin = _gameData.GetClampMin(Screen.width, Screen.height);
        _clampMax = _gameData.GetClampMax(Screen.width, Screen.height);

        _screenData = _gameData.DefaultScreenData;

        _undoStates = new UndoState[MaxUndoCount];
        for (var i = 0; i < _undoStates.Length; i++)
        {
            _undoStates[i] = new UndoState
            {
                PositionCount = 2,
                Positions = Enumerable.Repeat(Vector3.zero, 2).ToList()
            };
        }

        // _undoState = new UndoState
        // {
        //     PositionCount = 2,
        //     Positions = Enumerable.Repeat(Vector3.zero, 64).ToList()
        // };
    }

    private void InitializeLineRenderers()
    {
        // _linePoints = new List<Vector3[]>(_initialLineCount);
        _lineRenderers = new List<LineRenderer>(_initialLineCount);
        for (var i = 0; i < _initialLineCount; i++)
        {
            var go = new GameObject($"Line {i}");
            go.transform.SetParent(transform);

            // _linePoints.Add(new Vector3[256]);

            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            // lineRenderer.SetPositions(_linePoints[i]);
            lineRenderer.sharedMaterial = _lineMaterial;
            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;
            ResetLineRenderer(lineRenderer);

            _lineRenderers.Add(lineRenderer);
        }
    }

    private void Update()
    {
        if (!IsInputEnabled) return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Clear();
            return;
        }

        if (Keyboard.current.zKey.wasPressedThisFrame && _undoCount > 0)
        {
            _undoIndex = (_undoIndex - 1 + MaxUndoCount) % MaxUndoCount;
            _undoCount = Mathf.Max(0, _undoCount - 1);

            var undoState = _undoStates[_undoIndex];
            undoState.Rollback(MainRenderer);

            // _undoState.Write(MainRenderer);
            return;
        }

        if (_rendererIndex >= _lineRenderers.Count)
        {
            Debug.LogWarning("Renderer index out of range, reuse the shortest & oldest line.");
            var (index, renderer) = GetLineRendererToDiscard();
            ResetLineRenderer(renderer);
            _rendererIndex = index;
        }

        var mousePositionWorld = GetMousePositionWorldGrid(_unitHeight);
        _cursor.position = mousePositionWorld;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
#if UNITY_EDITOR
            var lineSerializer = new LineSerializer();
            var points = new List<Vector2Int> { new(0, 0), new(2, 0), new(1, 1) };
            var lineData = new LineData
            {
                IsLoop = false,
                Points = points
            };

            lineSerializer.BuildLine(lineData, _camera, _lineRenderers[^1], _screenData);

            var point = lineSerializer.WorldToPoint(_lineRenderers[^1].GetPosition(1), _camera, _screenData);
            Debug.Log($"point: {point}");

            // Debug.Log($"mouse position (world): {mousePositionWorld}; actual: {Mouse.current.position.value}");
            return;
#endif
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _tempLineRenderer.SetPosition(0, mousePositionWorld);
            Debug.Log($"mouse pressed: {mousePositionWorld}");
        }

        if (Mouse.current.leftButton.isPressed)
        {
            _tempLineRenderer.SetPosition(1, mousePositionWorld);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Debug.Log($"mouse relased: {mousePositionWorld}");

            if (IsLineRendererValid(_tempLineRenderer))
            {
                _undoStates[_undoIndex].Record(MainRenderer);

                _undoIndex = (_undoIndex + 1) % MaxUndoCount;
                _undoCount = Mathf.Min(_undoCount + 1, MaxUndoCount);

                // _undoState.Read(MainRenderer);
                Debug.Log($"created line.");

                var isNewLine = ProcessNewLine(_tempLineRenderer);
                if (isNewLine)
                {
                    if (IsNewLineAllowed)
                    {
                        var lineRenderer = _lineRenderers[_rendererIndex];
                        lineRenderer.SetPosition(0, _tempLineRenderer.GetPosition(0));
                        lineRenderer.SetPosition(1, _tempLineRenderer.GetPosition(1));

                        // force single line
                        // _rendererIndex++;
                    }
                    else
                    {
                        Debug.LogWarning("NEW LINE NOT ALLOWED.");
                    }
                }

                ResetLineRenderer(_tempLineRenderer);
            }
        }
    }

    // return true if it's a new line, false otherwise
    private bool ProcessNewLine(LineRenderer lineRenderer)
    {
        var currentLineStart = lineRenderer.GetPosition(0);
        var currentLineEnd = lineRenderer.GetPosition(1);

        // todo: check if the new line can be made just by moving a point in an old line
        // (starts or ends at a point between the two points of an old line)

        // todo: try to combine if lines with multiple points can be combined?

        // check if the new line forms a loop with an old line
        var lineRendererToLoop = GetLineRendererToLoop(currentLineStart, currentLineEnd);
        if (lineRendererToLoop != null && lineRendererToLoop != lineRenderer)
        {
            lineRendererToLoop.loop = true;
            return false;
        }

        LineRenderer lineRendererToContinue;
        // check if the new line starts at the end of another line
        lineRendererToContinue = GetLineRendererToContinue(currentLineStart);
        if (lineRendererToContinue != null && lineRendererToContinue != lineRenderer)
        {
            // append the end point
            var newPoint = currentLineEnd;
            lineRendererToContinue.positionCount++;
            lineRendererToContinue.SetPosition(lineRendererToContinue.positionCount - 1, newPoint);
            lineRendererToContinue.Simplify(_tolerance);

            return false;
        }

        // check if the new line ends at the end of another line
        lineRendererToContinue = GetLineRendererToContinue(currentLineEnd);
        if (lineRendererToContinue != null && lineRendererToContinue != lineRenderer)
        {
            // append the start point
            var newPoint = currentLineStart;
            lineRendererToContinue.positionCount++;
            lineRendererToContinue.SetPosition(lineRendererToContinue.positionCount - 1, newPoint);
            lineRendererToContinue.Simplify(_tolerance);

            return false;
        }

        return true;
    }

    [return: MaybeNull]
    private LineRenderer GetLineRendererToContinue(Vector2 position)
    {
        // iterate through line renderers
        // check if the end point matches the given position
        foreach (var lineRenderer in _lineRenderers)
        {
            if (lineRenderer.loop) continue;

            var lineEnd = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            if (IsWithinDistance(position, lineEnd))
            {
                return lineRenderer;
            }
        }

        return null;
    }

    [return: MaybeNull]
    private LineRenderer GetLineRendererToLoop(Vector2 positionStart, Vector2 positionEnd)
    {
        // iterate through line renderers
        // check if the end point matches the given position
        foreach (var lineRenderer in _lineRenderers)
        {
            var lineStart = lineRenderer.GetPosition(0);
            var lineEnd = lineRenderer.GetPosition(lineRenderer.positionCount - 1);
            if (IsWithinDistance(positionStart, lineStart) && IsWithinDistance(positionEnd, lineEnd) ||
                IsWithinDistance(positionEnd, lineStart) && IsWithinDistance(positionStart, lineEnd))
            {
                return lineRenderer;
            }
        }

        return null;
    }

    // return the shortest & oldest
    private (int, LineRenderer) GetLineRendererToDiscard()
    {
        var shortestLineRendererIndex = 0;
        var shortestLineRenderer = _lineRenderers[0];
        for (var i = 0; i < _lineRenderers.Count; i++)
        {
            var lineRenderer = _lineRenderers[i];
            if (lineRenderer.positionCount < shortestLineRenderer.positionCount)
            {
                shortestLineRenderer = lineRenderer;
                shortestLineRendererIndex = i;
            }
        }

        return (shortestLineRendererIndex, shortestLineRenderer);
    }

    private void ResetLineRenderer(LineRenderer lineRenderer)
    {
        for (var i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i, Vector3.zero);
        }
    }

    // valid if the points aren't too close
    private bool IsLineRendererValid(LineRenderer lineRenderer, float threshold = 0.1f)
    {
        var p0 = lineRenderer.GetPosition(0);
        var p1 = lineRenderer.GetPosition(1);

        return !IsWithinDistance(p0, p1, threshold);
    }

    private bool IsWithinDistance(Vector2 p0, Vector2 p1, float threshold = 0.1f)
    {
        return (p0 - p1).sqrMagnitude < threshold * threshold;
    }

    private Vector2 GetMousePositionWorldGrid(int roundTo = 0)
    {
        var mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition.x = Mathf.Clamp(mouseScreenPosition.x, _clampMin.x, _clampMax.x);
        mouseScreenPosition.y = Mathf.Clamp(mouseScreenPosition.y, _clampMin.y, _clampMax.y);

        if (roundTo > 0)
        {
            // 432.1 -> 430
            // 438.2 -> 440
            var x = Mathf.RoundToInt(mouseScreenPosition.x / roundTo) * roundTo;
            var y = Mathf.RoundToInt(mouseScreenPosition.y / roundTo) * roundTo;
            mouseScreenPosition = new Vector2(x, y);
        }

        var mousePositionWorldGrid = _camera.ScreenToWorldPoint(mouseScreenPosition);
        // mousePositionWorldGrid.x = Mathf.Clamp(mousePositionWorldGrid.x, -_clamp.x, _clamp.x);
        // mousePositionWorldGrid.y = Mathf.Clamp(mousePositionWorldGrid.y, -_clamp.y, _clamp.y);
        return mousePositionWorldGrid;
    }

    public void Clear()
    {
        foreach (var lineRenderer in _lineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }

        ResetLineRenderer(_tempLineRenderer);
        InitializeLineRenderers();
        _rendererIndex = 0;
        // IsNewLineAllowed = true;
        // _undoState.Reset();

        foreach (var undoState in _undoStates)
        {
            undoState.Reset();
        }
    }

    private sealed class UndoState
    {
        public bool WasLoop;
        public int PositionCount;
        public List<Vector3> Positions;

        public void Record(LineRenderer lineRenderer)
        {
            PositionCount = lineRenderer.positionCount;
            WasLoop = lineRenderer.loop;

            var elementsToAdd = Mathf.Max(0, PositionCount - Positions.Count);
            for (var i = 0; i < elementsToAdd; i++)
            {
                Positions.Add(Vector3.zero);
            }

            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                Positions[i] = lineRenderer.GetPosition(i);
            }
        }

        public void Rollback(LineRenderer lineRenderer)
        {
            lineRenderer.loop = WasLoop;
            lineRenderer.positionCount = PositionCount;
            for (var i = 0; i < lineRenderer.positionCount; i++)
            {
                lineRenderer.SetPosition(i, Positions[i]);
            }
        }

        public void Reset()
        {
            PositionCount = 2;
            for (var i = 0; i < Positions.Count; i++)
            {
                Positions[i] = Vector3.zero;
            }
        }
    }
}
