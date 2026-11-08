using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public struct LineSegment : IEquatable<LineSegment>
{
    public Vector2Int Start;
    public Vector2Int End;

    public int SubSegmentCount
    {
        get
        {
            var (dx, dy) = SubSegmentUnits;
            var subSegmentCount = dy == 0 ? (End.x - Start.x) / dx : (End.y - Start.y) / dy;
            return subSegmentCount;
        }
    }

    private (int dx, int dy) SubSegmentUnits
    {
        get
        {
            int dx, dy;
            var slope = Slope;
            if (float.IsPositiveInfinity(slope))
            {
                dx = 0;
                dy = 1;
            }
            else if (float.IsNegativeInfinity(slope))
            {
                dx = 0;
                dy = -1;
            }
            else if (Mathf.Approximately(Mathf.RoundToInt(slope), slope))
            {
                dx = 1;
                dy = Mathf.RoundToInt(slope);
            }
            else if (slope > 0 && slope < 1)
            {
                dx = Mathf.RoundToInt(1f / slope);
                dy = 1;
            }
            else if (slope > -1 && slope < 0)
            {
                dx = Mathf.RoundToInt(1f / -slope);
                dy = -1;
            }
            else
            {
                dx = End.x - Start.x;
                dy = End.y - Start.y;
            }

            return (dx, dy);
        }
    }

    public float Slope => (float)(End.y - Start.y) / (End.x - Start.x);

    public LineSegment(Vector2Int p0, Vector2Int p1)
    {
        Assert.AreNotEqual(p0, p1);

        if (p0.x < p1.x)
        {
            Start = p0;
            End = p1;
        }
        else if (p1.x < p0.x)
        {
            Start = p1;
            End = p0;
        }
        // p0.x == p1.x below
        else if (p0.y < p1.y)
        {
            Start = p0;
            End = p1;
        }
        else
        {
            Start = p1;
            End = p0;
        }
    }

    public SubSegmentEnumerator GetSubSegments()
    {
        return new SubSegmentEnumerator(this);
    }

    public bool Equals(LineSegment other)
    {
        return Start.Equals(other.Start) && End.Equals(other.End);
    }

    public override bool Equals(object obj)
    {
        return obj is LineSegment other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Start, End);
    }

    public static bool operator ==(LineSegment left, LineSegment right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(LineSegment left, LineSegment right)
    {
        return !left.Equals(right);
    }

    public override string ToString()
    {
        return $"({Start.x},{Start.y})-({End.x},{End.y})";
    }

    public readonly struct SubSegmentEnumerator : IEnumerable<LineSegment>
    {
        private readonly Enumerator _enumerator;

        internal SubSegmentEnumerator(LineSegment parent)
        {
            _enumerator = new Enumerator(parent);
        }

        public Enumerator GetEnumerator() => _enumerator;
        IEnumerator<LineSegment> IEnumerable<LineSegment>.GetEnumerator() => GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public struct Enumerator : IEnumerator<LineSegment>
    {
        private readonly LineSegment _parent;
        private readonly int _dx;
        private readonly int _dy;

        private int _index;
        private LineSegment _current;

        internal Enumerator(LineSegment parent)
        {
            _parent = parent;
            _index = 0;
            (_dx, _dy) = _parent.SubSegmentUnits;
            _current = default;
        }

        public bool MoveNext()
        {
            var x = _parent.Start.x;
            var y = _parent.Start.y;

            var p0 = new Vector2Int(x + _dx * _index, y + _dy * _index);
            if (p0.x >= _parent.End.x && p0.y >= _parent.End.y)
            {
                _current = default;
                return false;
            }

            var p1 = new Vector2Int(x + _dx * (_index + 1), y + _dy * (_index + 1));

            _current = new LineSegment(p0, p1);
            _index++;

            return true;
        }

        public void Reset()
        {
            _index = 0;
            _current = default;
        }

        public LineSegment Current => _current;

        object IEnumerator.Current => Current;

        public void Dispose() { }
    }
}
