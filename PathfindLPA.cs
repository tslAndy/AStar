class PathfindLPA : Pathfinder
{
    private readonly Dictionary<Vec2Int, int> _field; // int is g cost
    private readonly Heap<(int, int), Vec2Int> _heap;
    private readonly IComparer<(int, int)> _comp;

    private Vec2Int _start,
        _end;

    // can not use int.MaxValue because in RHS
    // gCost + h will cause num overflow and drop to negative
    private const int BIG_NUM = 10_000_000;

    public PathfindLPA(int width, int height)
        : base(width, height)
    {
        _field = new Dictionary<Vec2Int, int>();
        _comp = new ComparerAB();
        _heap = new Heap<(int, int), Vec2Int>(_comp);
    }

    public void SetPoints(Vec2Int start, Vec2Int end)
    {
        _start = start;
        _end = end;

        _field[_start] = BIG_NUM;
        _field[_end] = BIG_NUM;
        _heap.Add(_start, (GetCost(_start, _end), 0));
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        if (_start != start || _end != end)
        {
            _field.Clear();
            _heap.Clear();
            SetPoints(start, end);
        }

        while (
            _heap.TryPeekWithKey(out Vec2Int pos, out (int, int) key)
            && (_comp.Compare(key, GetKey(_end)) < 0 || GetRhs(_end) != _field[end])
        )
        {
            if (_field[pos] > GetRhs(pos))
            {
                _heap.Pop();
                _field[pos] = GetRhs(pos);
                for (int ty = pos.y - 1; ty < pos.y + 2; ty++)
                for (int tx = pos.x - 1; tx < pos.x + 2; tx++)
                    if (IsCorrect(new Vec2Int(tx, ty)) && tx != 0 && ty != 0)
                        UpdateVertex(new Vec2Int(tx, ty));
            }
            else
            {
                _field[pos] = BIG_NUM;
                for (int ty = pos.y - 1; ty < pos.y + 2; ty++)
                for (int tx = pos.x - 1; tx < pos.x + 2; tx++)
                    if (IsCorrect(new Vec2Int(tx, ty)) && tx != 0 && ty != 0)
                        UpdateVertex(new Vec2Int(tx, ty));
                UpdateVertex(pos);
            }
        }

        List<Vec2Int> points = new List<Vec2Int>();
        Vec2Int temp = _end;
        while (temp != start)
        {
            points.Add(temp);

            Vec2Int next = GetPredPoint(temp);
            if (next == temp)
                return default;

            temp = next;
        }
        points.Add(start);
        points.Reverse();

        int length = 0;
        for (int i = 0; i < points.Count - 1; i++)
            length += GetCost(points[i], points[i + 1]);

        return new Path(points.ToArray(), length);
    }

    private void UpdateVertex(Vec2Int pos)
    {
        if ((!IsCorrect(pos)) || this[pos])
            return;

        int rhs = GetRhs(pos);
        if (_field.TryGetValue(pos, out int gCost))
        {
            _heap.TryRemove(pos);
            if (gCost != rhs)
                _heap.Add(pos, GetKey(pos));
        }
        else
        {
            _field[pos] = BIG_NUM;
            _heap.Add(pos, GetKey(pos));
        }
    }

    private int GetRhs(Vec2Int pos)
    {
        if (pos == _start)
            return 0;

        int min = int.MaxValue;
        for (int ty = pos.y - 1; ty < pos.y + 2; ty++)
        {
            for (int tx = pos.x - 1; tx < pos.x + 2; tx++)
            {
                Vec2Int near = new Vec2Int(tx, ty);
                if (near != pos && IsCorrect(near) && _field.TryGetValue(near, out int gCost))
                    min = Math.Min(min, gCost + GetCost(pos, near));
            }
        }

        return min;
    }

    private (int, int) GetKey(Vec2Int pos)
    {
        int min = Math.Min(_field[pos], GetRhs(pos));
        int cost = GetCost(pos, _end);
        return (min + cost, min);
    }

    private Vec2Int GetPredPoint(Vec2Int pos)
    {
        Vec2Int point = pos;
        int minCostG = BIG_NUM;

        for (int ty = pos.y - 1; ty < pos.y + 2; ty++)
        {
            for (int tx = pos.x - 1; tx < pos.x + 2; tx++)
            {
                Vec2Int near = new Vec2Int(tx, ty);

                if (
                    near != pos
                    && IsCorrect(near)
                    && _field.TryGetValue(near, out int gCost)
                    && gCost < minCostG
                )
                {
                    minCostG = gCost;
                    point = near;
                }
            }
        }

        return point;
    }

    public override bool this[Vec2Int pos]
    {
        get => base[pos];
        set
        {
            base[pos] = value;

            if (value) // adding obstacle
            {
                // если клетка раньше обрабатывалась
                if (_field.ContainsKey(pos))
                    _field.Remove(pos);
                _heap.TryRemove(pos);
            }
            else
            {
                UpdateVertex(pos);
            }

            for (int ty = pos.y - 1; ty < pos.y + 2; ty++)
            for (int tx = pos.x - 1; tx < pos.x + 2; tx++)
                UpdateVertex(new Vec2Int(tx, ty));
        }
    }
}
