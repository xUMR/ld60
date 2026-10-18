using System;
using System.Collections.Generic;
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

        // check every segment
        var pairs = new HashSet<(Vector2Int p0, Vector2Int p1)>();
        for (var i = 0; i < Points.Count - 1; i++)
        {
            // ensure stable ordering
            if (Points[i].sqrMagnitude < Points[i + 1].sqrMagnitude)
            {
                pairs.Add((Points[i], Points[i + 1]));
            }
            else
            {
                pairs.Add((Points[i + 1], Points[i]));
            }
        }

        var otherPairs = new HashSet<(Vector2Int p0, Vector2Int p1)>();
        for (var i = 0; i < other.Points.Count - 1; i++)
        {
            // ensure stable ordering
            if (other.Points[i].sqrMagnitude < other.Points[i + 1].sqrMagnitude)
            {
                otherPairs.Add((other.Points[i], other.Points[i + 1]));
            }
            else
            {
                otherPairs.Add((other.Points[i + 1], other.Points[i]));
            }
        }

        pairs.ExceptWith(otherPairs);

        return pairs.Count == 0;


        for (var i = 0; i < other.Points.Count - 1; i++)
        {
            pairs.Remove((other.Points[i], other.Points[i + 1]));
            pairs.Remove((other.Points[i + 1], other.Points[i]));
        }

        return pairs.Count > 0;
    }
}
