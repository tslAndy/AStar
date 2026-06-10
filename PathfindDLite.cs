class PathfindDLite : Pathfinder
{
    private Heap<(int, int), Vec2Int> _heap;
    private Dictionary<Vec2Int, LNode> _field;
    private IComparer<(int, int)> _comp;

    private int _km;
    private Vec2Int _start,
        _end;

    private const int BIG_NUM = 100_000_000;

    public PathfindDLite(int width, int height)
        : base(width, height)
    {
        _comp = new ComparerAB();
        _heap = new Heap<(int, int), Vec2Int>(_comp);
        _field = new Dictionary<Vec2Int, LNode>();
    }

    public override bool this[Vec2Int pos]
    {
        get => base[pos];
        set
        {
            base[pos] = value;

            _field[pos] = new LNode(BIG_NUM);
            if (value)
                _heap.TryRemove(pos);
            else
                _field.Remove(pos);

            for (int dy = -1; dy < 2; dy++)
            for (int dx = -1; dx < 2; dx++)
                UpdateVertex(pos + new Vec2Int(dx, dy));
        }
    }

    private void Init(Vec2Int start, Vec2Int end)
    {
        _heap.Clear();
        _field.Clear();

        _km = 0;
        _start = start;
        _end = end;

        _field[_start] = new LNode(BIG_NUM);
        _field[_end] = new LNode(BIG_NUM);

        _heap.Add(_end, (GetCost(start, end), 0));
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        if (start == GetMinPred(_start) && _end == end)
        {
            _km += GetCost(_start, start);
            _start = start;
        }
        else
        {
            Init(start, end);
        }

        while (
            _heap.TryPopWithKey(out Vec2Int pos, out (int, int) key)
            && (_comp.Compare(key, GetKey(start)) < 0 || GetRHS(start) != _field[start].g)
        )
        {
            if (_comp.Compare(key, GetKey(pos)) < 0)
            {
                _heap.Add(pos, GetKey(pos));
            }
            else if (_field[pos].g > GetRHS(pos))
            {
                _field[pos] = new LNode(GetRHS(pos));
                for (int dy = -1; dy < 2; dy++)
                for (int dx = -1; dx < 2; dx++)
                    if (dx != 0 || dy != 0)
                        UpdateVertex(pos + new Vec2Int(dx, dy));
            }
            else
            {
                _field[pos] = new LNode(BIG_NUM);
                for (int dy = -1; dy < 2; dy++)
                for (int dx = -1; dx < 2; dx++)
                    UpdateVertex(pos + new Vec2Int(dx, dy));
            }
        }

        if (_field[_start].g == BIG_NUM)
            return default;

        List<Vec2Int> points = new List<Vec2Int>();
        Vec2Int temp = _start;
        while (temp != _end)
        {
            points.Add(temp);
            Vec2Int next = GetMinPred(temp);
            temp = next;
        }
        points.Add(_end);
        points.Reverse();
        return new Path(points.ToArray(), 0);
    }

    private void UpdateVertex(Vec2Int pos)
    {
        if ((!IsCorrect(pos)) || this[pos])
            return;

        if (!_field.TryGetValue(pos, out LNode node))
            _field[pos] = node = new LNode(BIG_NUM);

        int rhs = GetRHS(pos);
        if (node.g == rhs)
            _heap.TryRemove(pos);
        else
            _heap.AddOrChange(pos, GetKey(pos));
    }

    private int GetRHS(Vec2Int pos)
    {
        if (pos == _end)
            return 0;

        int min = BIG_NUM;
        for (int dy = -1; dy < 2; dy++)
        {
            for (int dx = -1; dx < 2; dx++)
            {
                Vec2Int near = pos + new Vec2Int(dx, dy);
                if (
                    near != pos
                    && IsCorrect(near)
                    && (!this[near])
                    && _field.TryGetValue(near, out LNode node)
                )
                    min = Math.Min(min, node.g + GetCost(pos, near));
            }
        }
        return min;
    }

    private Vec2Int GetMinPred(Vec2Int pos)
    {
        Vec2Int minPred = pos;
        int minCost = BIG_NUM;

        for (int dy = -1; dy < 2; dy++)
        {
            for (int dx = -1; dx < 2; dx++)
            {
                Vec2Int near = pos + new Vec2Int(dx, dy);
                if (
                    near != pos
                    && IsCorrect(near)
                    && (!this[near])
                    && _field.TryGetValue(near, out LNode node)
                    && node.g < minCost
                )
                {
                    minPred = near;
                    minCost = node.g;
                }
            }
        }

        return minPred;
    }

    private (int, int) GetKey(Vec2Int pos)
    {
        int min = Math.Min(_field[pos].g, GetRHS(pos));
        return (min + GetCost(pos, _start) + _km, min);
    }

    private record struct LNode(int g);
}
