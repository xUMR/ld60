using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

    [SerializeField] private int _resolution = 36; // vertical units, assume 16:9 screen
    private int _unitHeight;

    [SerializeField] private int _initialLineCount = 8;
    [SerializeField] private float _lineWidth = 0.01f;
    [SerializeField] private float _tolerance = 0.01f;

    [SerializeField] private Material _lineMaterial;
    [SerializeField] private List<LineRenderer> _lineRenderers;
    [SerializeField] private Transform _cursor;

    private int _rendererIndex;
    private Camera _camera;

    private Vector2 _currentLineStart;
    private Vector2 _currentLineEnd;

    public int Resolution => _resolution;

    private void Awake()
    {
        Cursor.visible = false;

        InitializeLineRenderers();

        _camera = Camera.main;
        _unitHeight = Screen.height / _resolution;
    }

    private void InitializeLineRenderers()
    {
        _lineRenderers = new List<LineRenderer>(_initialLineCount);
        for (var i = 0; i < _initialLineCount; i++)
        {
            var go = new GameObject($"Line {i}");
            go.transform.SetParent(transform);
            var lineRenderer = go.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            lineRenderer.sharedMaterial = _lineMaterial;
            lineRenderer.startWidth = _lineWidth;
            lineRenderer.endWidth = _lineWidth;
            ResetLineRenderer(lineRenderer);

            _lineRenderers.Add(lineRenderer);
        }
    }

    private void Update()
    {
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            Clear();
            return;
        }

        if (_rendererIndex >= _lineRenderers.Count)
        {
            Debug.LogWarning("Renderer index out of range, reuse the shortest & oldest line.");
            var (index, renderer) = GetLineRendererToDiscard();
            ResetLineRenderer(renderer);
            _rendererIndex = index;
        }

        var lineRenderer = _lineRenderers[_rendererIndex];
        if (lineRenderer == null)
        {
            Debug.LogError("No line renderer available.");
            return;
        }

        var mousePositionWorld = GetMousePositionWorldGrid(_unitHeight);
        _cursor.position = mousePositionWorld;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _currentLineStart = mousePositionWorld;
            lineRenderer.SetPosition(0, mousePositionWorld);
            Debug.Log($"mouse pressed: {mousePositionWorld}");
        }

        if (Mouse.current.leftButton.isPressed)
        {
            lineRenderer.SetPosition(1, mousePositionWorld);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            _currentLineEnd = mousePositionWorld;
            Debug.Log($"mouse relased: {mousePositionWorld}");

            if (IsLineRendererValid(lineRenderer))
            {
                Debug.Log($"created line.");

                var currentLineStart = _currentLineStart;
                var currentLineEnd = _currentLineEnd;
                var isNewLine = ProcessNewLine(lineRenderer, currentLineStart, currentLineEnd);
                if (isNewLine)
                {
                    _rendererIndex++;
                }
                else
                {
                    ResetLineRenderer(lineRenderer);
                }

                _currentLineStart = Vector2.zero;
            }
        }
    }

    // return true if it's a new line, false otherwise
    private bool ProcessNewLine(LineRenderer lineRenderer, Vector2 currentLineStart, Vector2 currentLineEnd)
    {
        // todo: check if the new line can be made just by moving a point in an old line
        // (starts or ends at a point between the two points of an old line)

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
        if (roundTo > 0)
        {
            // 432.1 -> 430
            // 438.2 -> 440
            var x = Mathf.RoundToInt(mouseScreenPosition.x / roundTo) * roundTo;
            var y = Mathf.RoundToInt(mouseScreenPosition.y / roundTo) * roundTo;
            mouseScreenPosition = new Vector2(x, y);
        }

        return _camera.ScreenToWorldPoint(mouseScreenPosition);
    }

    private void Clear()
    {
        foreach (var lineRenderer in _lineRenderers)
        {
            Destroy(lineRenderer.gameObject);
        }

        InitializeLineRenderers();
        _rendererIndex = 0;
    }
}
