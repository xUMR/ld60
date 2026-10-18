using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class LineSerializer
{
    public Vector2 PointToWorld(Vector2Int point, Camera camera, ScreenData screenData)
    {
        Assert.IsTrue(point.x >= 0);
        Assert.IsTrue(point.y >= 0);

        // (0,0) -> (-9.6,-7.2)
        // (0,1) -> (-9.6,-6.6)
        // (1,0) -> (-9.0,-7.2)
        // (1,1) -> (-9.0,-6.6)

        var result = new Vector2(
            screenData.ClampMin.x + point.x * screenData.Unit,
            screenData.ClampMin.y + point.y * screenData.Unit);

        Assert.IsTrue(result.x <= screenData.ClampMax.x);
        Assert.IsTrue(result.y <= screenData.ClampMax.y);

        return camera.ScreenToWorldPoint(result);
    }

    public Vector2Int WorldToPoint(Vector2 position, Camera camera, ScreenData screenData)
    {
        var screenPoint = camera.WorldToScreenPoint(position);

        var result = new Vector2Int(
            Mathf.RoundToInt((screenPoint.x - screenData.ClampMin.x) / screenData.Unit),
            Mathf.RoundToInt((screenPoint.y - screenData.ClampMin.y) / screenData.Unit));

        return result;
    }

    public void BuildLine(LineData lineData, Camera camera, LineRenderer lineRenderer, ScreenData screenData)
    {
        var points = lineData.Points;
        lineRenderer.positionCount = points.Count;
        for (var i = 0; i < points.Count; i++)
        {
            var point = points[i];
            var position = PointToWorld(point, camera, screenData);
            lineRenderer.SetPosition(i, position);
        }

        // todo magic value tolerance
        lineRenderer.Simplify(0.01f);
        lineRenderer.loop = lineData.IsLoop;
    }

    public LineData GetLineData(LineRenderer lineRenderer, Camera camera, ScreenData screenData)
    {
        var points =  new List<Vector2Int>(lineRenderer.positionCount);

        for (var i = 0; i < lineRenderer.positionCount; i++)
        {
            var position = lineRenderer.GetPosition(i);
            points.Add(WorldToPoint(position, camera, screenData));
        }

        return new LineData
        {
            IsLoop = lineRenderer.loop,
            Points = points,
        };
    }

    public string ToJson(LineData lineData)
    {
        return JsonUtility.ToJson(lineData);
    }

    public LineData FromJson(string json)
    {
        return JsonUtility.FromJson<LineData>(json);
    }
}
