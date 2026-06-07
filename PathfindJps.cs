class PathfindJps : Pathfinder
{
    private readonly Dictionary<Vec2Int, Node> _field;
    private readonly HashSet<Vec2Int> _closed;
    private readonly Heap<(int, int), Vec2Int> _heap;

    public PathfindJps(int width, int height)
        : base(width, height)
    {
        _field = new Dictionary<Vec2Int, Node>();
        _closed = new HashSet<Vec2Int>();
        _heap = new Heap<(int, int), Vec2Int>(new ComparerAB());
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        _field.Clear();
        _closed.Clear();
        _heap.Clear();

        Node first = new Node(0, GetCost(start, end), start);
        _field.Add(start, first);
        _heap.Add(start, (first.fCost, first.hCost));

        while (_heap.Count != 0)
        {
            Vec2Int pos = _heap.Pop();
            Node node = _field[pos];

            _closed.Add(pos);

            if (pos == end)
                break;

            Vec2Int dir = (pos - node.prev).Clamp(-1, 1);
            (Pruned pruned, int count) = GetPruned(pos, dir);
            for (int i = 0; i < count; i++)
            {
                Vec2Int prunedDir = pruned[i];
                if ((!Trace(pos, prunedDir, end, out Vec2Int jump)) || _closed.Contains(jump))
                    continue;

                Node jumpNode = new Node(node.gCost + GetCost(pos, jump), GetCost(jump, end), pos);
                if (_field.TryGetValue(jump, out Node existing))
                {
                    if (jumpNode.fCost < existing.fCost)
                    {
                        _field[jump] = jumpNode;
                        _heap.Change(jump, (jumpNode.fCost, jumpNode.hCost));
                    }
                }
                else
                {
                    _field.Add(jump, jumpNode);
                    _heap.Add(jump, (jumpNode.fCost, jumpNode.hCost));
                }
            }
        }

        if (_closed.Contains(end))
            return BuildPath(_field, start, end);
        return default;
    }

    private bool Trace(Vec2Int pos, Vec2Int dir, Vec2Int end, out Vec2Int result)
    {
        if (dir == Vec2Int.zero)
            throw new Exception("Dir can not be zero");

        if (dir.x != 0 && dir.y != 0)
            return TraceDiag(pos, dir, end, out result);
        else
            return TraceAxis(pos, dir, end, out result);
    }

    private bool TraceDiag(Vec2Int pos, Vec2Int dir, Vec2Int end, out Vec2Int result)
    {
        pos += dir;
        while (IsCorrect(pos) && !this[pos])
        {
            if (pos == end)
            {
                result = pos;
                return true;
            }

            (Forced forced, int count) = GetForced(pos, dir);
            if (count != 0)
            {
                result = pos;
                return true;
            }

            if (
                TraceAxis(pos, dir.OnlyX, end, out result)
                || TraceAxis(pos, dir.OnlyY, end, out result)
            )
            {
                result = pos;
                return true;
            }

            pos += dir;
        }

        result = pos;
        return false;
    }

    private bool TraceAxis(Vec2Int pos, Vec2Int dir, Vec2Int end, out Vec2Int result)
    {
        pos += dir;
        while (IsCorrect(pos) && !this[pos])
        {
            if (pos == end)
            {
                result = end;
                return true;
            }

            (Forced forced, int count) = GetForced(pos, dir);
            if (count != 0)
            {
                result = pos;
                return true;
            }

            pos += dir;
        }

        result = pos;
        return false;
    }

    protected (Pruned, int) GetPruned(Vec2Int pos, Vec2Int dir)
    {
        Pruned pruned = new Pruned();
        int count = 0;

        if (dir == Vec2Int.zero)
        {
            pruned[0] = Vec2Int.up;
            pruned[1] = Vec2Int.down;
            pruned[2] = Vec2Int.left;
            pruned[3] = Vec2Int.right;
            pruned[4] = Vec2Int.upLeft;
            pruned[5] = Vec2Int.upRight;
            pruned[6] = Vec2Int.downLeft;
            pruned[7] = Vec2Int.downRight;
            return (pruned, 8);
        }

        (Forced forced, int forcedCount) = GetForced(pos, dir);
        for (int i = 0; i < forcedCount; i++)
            pruned[count++] = forced[i];

        pruned[count++] = dir;
        if (dir.x != 0 && dir.y != 0)
        {
            pruned[count++] = dir.OnlyX;
            pruned[count++] = dir.OnlyY;
        }

        return (pruned, count);
    }

    protected (Forced, int) GetForced(Vec2Int pos, Vec2Int dir)
    {
        Forced forced = new Forced();
        int count = 0;

        if (dir.x != 0 && dir.y != 0)
        {
            Vec2Int diagA = pos + new Vec2Int(-dir.x, 0);
            Vec2Int diagAN = pos + new Vec2Int(-dir.x, dir.y);
            if (IsCorrect(diagA) && IsCorrect(diagAN) && this[diagA] && !this[diagAN])
                forced[count++] = new Vec2Int(-dir.x, dir.y);

            Vec2Int diagB = pos + new Vec2Int(0, -dir.y);
            Vec2Int diagBN = pos + new Vec2Int(dir.x, -dir.y);
            if (IsCorrect(diagB) && IsCorrect(diagBN) && this[diagB] && !this[diagBN])
                forced[count++] = new Vec2Int(dir.x, -dir.y);
        }
        else if (dir.x != 0)
        {
            Vec2Int up = pos + Vec2Int.up;
            Vec2Int upDiag = up + dir;
            if (IsCorrect(up) && IsCorrect(upDiag) && this[up] && !this[upDiag])
                forced[count++] = new Vec2Int(dir.x, 1);

            Vec2Int down = pos + Vec2Int.down;
            Vec2Int downDiag = down + dir;
            if (IsCorrect(down) && IsCorrect(downDiag) && this[down] && !this[downDiag])
                forced[count++] = new Vec2Int(dir.x, -1);
        }
        else if (dir.y != 0)
        {
            Vec2Int right = pos + Vec2Int.right;
            Vec2Int rightDiag = right + dir;
            if (IsCorrect(right) && IsCorrect(rightDiag) && this[right] && !this[rightDiag])
                forced[count++] = new Vec2Int(1, dir.y);

            Vec2Int left = pos + Vec2Int.left;
            Vec2Int leftDiag = left + dir;
            if (IsCorrect(left) && IsCorrect(leftDiag) && this[left] && !this[leftDiag])
                forced[count++] = new Vec2Int(-1, dir.y);
        }

        return (forced, count);
    }

    [System.Runtime.CompilerServices.InlineArray(2)]
    protected struct Forced
    {
        private Vec2Int pos;
    }

    [System.Runtime.CompilerServices.InlineArray(8)]
    protected struct Pruned
    {
        private Vec2Int pos;
    }
}
