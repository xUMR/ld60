using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class LineData
{
    public bool IsLoop;
    public List<Vector2Int> Points;

    public bool Matches(LineData other)
    {
        // todo this is incorrect but let's keep it for now
        if (IsLoop != other.IsLoop)
            return false;
/*
        if (Points.Count == other.Points.Count)
        {
            // the points can be in the same order or reversed
            // also the starting point can change
            var otherStartIndex = other.Points.IndexOf(Points[0]);
            if (otherStartIndex < 0)
                return false;

            var pointsMatch = true;
            for (var i = 0; i < Points.Count; i++)
            {
                var j = (otherStartIndex + i) % Points.Count;
                if (Points[i] != other.Points[j])
                {
                    pointsMatch = false;
                    break;
                }
            }

            if (pointsMatch)
                return true;

            // now check the reverse order
            pointsMatch = true;
            for (var i = 0; i < Points.Count; i++)
            {
                var j = (otherStartIndex - i + Points.Count) % Points.Count;
                if (Points[i] != other.Points[j])
                {
                    pointsMatch = false;
                    break;
                }
            }

            if (pointsMatch)
                return true;
        }
        */

        // var pointSet = new HashSet<Vector2Int>(Points);
        // var otherPointSet = new HashSet<Vector2Int>(other.Points);
        //
        // return pointSet.SetEquals(otherPointSet);

        // check every segment
        var pairs = new HashSet<(Vector2Int p0, Vector2Int p1)>();
        for (var i = 0; i < Points.Count; i++)
        {
            var p0 = Points[i];
            var p1 = Points[(i + 1) % Points.Count];
            if (p0 == p1)
                continue;

            // ensure stable ordering
            var (v0, v1) = OrderVector2Int(p0, p1);
            pairs.Add((v0, v1));
        }

        var otherPairs = new HashSet<(Vector2Int p0, Vector2Int p1)>();
        for (var i = 0; i < other.Points.Count; i++)
        {
            var p0 = other.Points[i];
            var p1 = other.Points[(i + 1) % other.Points.Count];
            if (p0 == p1)
                continue;

            // ensure stable ordering
            var (v0, v1) = OrderVector2Int(p0, p1);
            otherPairs.Add((v0, v1));
        }

        var result = pairs.SetEquals(otherPairs);
        // if (!result)
        // {
#if UNITY_EDITOR
            Debug.Log(string.Join(", ", pairs));
            Debug.Log(string.Join(", ", otherPairs));
#endif
        // }

        return result;
    }

    private (Vector2Int, Vector2Int) OrderVector2Int(Vector2Int v0, Vector2Int v1)
    {
        var xComparison = v0.x.CompareTo(v1.x);
        switch (xComparison)
        {
            case 0:
            {
                var yComparison = v0.y.CompareTo(v1.y);
                if (yComparison > 0)
                    return (v1, v0);
                break;
            }
            case > 0:
                return (v1, v0);
        }

        return (v0, v1);
    }
}
