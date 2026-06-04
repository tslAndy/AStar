class PathfindJpsCached : Pathfinder
{
    private readonly Cell[] _cells;
    private readonly Dictionary<Vec2Int, Node> _field;
    private readonly HashSet<Vec2Int> _closed;
    private readonly Heap<Vec2Int> _heap;

    public PathfindJpsCached(int width, int height)
        : base(width, height)
    {
        _cells = new Cell[width * height];
        _field = new Dictionary<Vec2Int, Node>();
        _closed = new HashSet<Vec2Int>();
        _heap = new Heap<Vec2Int>();

        Update();
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        _field.Clear();
        _closed.Clear();
        _heap.Clear();

        Node first = new Node(0, GetCost(start, end), start);
        _field.Add(start, first);
        _heap.Add(start, first.fCost);

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
                if (
                    (!TraceRuntime(pos, prunedDir, end, out Vec2Int jump)) || _closed.Contains(jump)
                )
                    continue;

                Node jumpNode = new Node(node.gCost + GetCost(pos, jump), GetCost(jump, end), pos);
                if (_field.TryGetValue(jump, out Node existing))
                {
                    if (jumpNode.fCost < existing.fCost)
                    {
                        _heap.Change(jump, jumpNode.fCost);
                        _field[jump] = jumpNode;
                    }
                }
                else
                {
                    _field.Add(jump, jumpNode);
                    _heap.Add(jump, jumpNode.fCost);
                }
            }
        }

        if (_closed.Contains(end))
            return BuildPath(_field, start, end);
        return default;
    }

    private bool TraceRuntime(Vec2Int pos, Vec2Int dir, Vec2Int end, out Vec2Int result)
    {
        if (dir == Vec2Int.zero)
            throw new Exception("Dir can not be zero");

        if (dir.x != 0 && dir.y != 0)
            return TraceDiagRuntime(pos, dir, end, out result);
        else
            return TraceAxisRuntime(pos, dir, end, out result);
    }

    private bool TraceAxisRuntime(Vec2Int pos, Vec2Int dir, Vec2Int targ, out Vec2Int result)
    {
        pos += dir;
        if ((!IsCorrect(pos)) || this[pos])
        {
            result = default;
            return false;
        }

        Vec2Int end = pos + this[pos, dir] * dir;
        if (IsOnLine(targ, pos, end))
        {
            result = targ;
            return true;
        }

        (Forced forced, int count) = GetForced(pos, dir);
        if (count != 0)
        {
            result = pos;
            return true;
        }

        if (IsCorrect(end) && !this[end])
        {
            result = end;
            return true;
        }

        result = default;
        return false;
    }

    private bool TraceDiagRuntime(Vec2Int pos, Vec2Int dir, Vec2Int targ, out Vec2Int result)
    {
        pos += dir;
        if ((!IsCorrect(pos)) || this[pos])
        {
            result = default;
            return false;
        }

        Vec2Int end = pos + this[pos, dir] * dir;
        if (IsOnLine(targ, pos, end))
        {
            result = targ;
            return true;
        }

        (Forced forced, int count) = GetForced(pos, dir);
        if (count != 0)
        {
            result = pos;
            return true;
        }

        if (
            TraceAxisRuntime(pos, dir.OnlyX, targ, out result)
            || TraceAxisRuntime(pos, dir.OnlyY, targ, out result)
        )
        {
            result = pos;
            return true;
        }

        int distX = Math.Abs(targ.x - pos.x);
        int distY = Math.Abs(targ.y - pos.y);

        Vec2Int hitX = pos + dir * distX;
        Vec2Int hitY = pos + dir * distY;

        bool qxCorrect =
            Math.Min(pos.x, end.x) <= targ.x
            && targ.x <= Math.Max(pos.x, end.x)
            && IsCorrect(hitX)
            && (!this[hitX]);

        bool qyCorrect =
            Math.Min(pos.y, end.y) <= targ.y
            && targ.y <= Math.Max(pos.y, end.y)
            && IsCorrect(hitY)
            && (!this[hitY]);

        if (qxCorrect && qyCorrect)
        {
            result = distX <= distY ? hitX : hitY;
            return true;
        }
        else if (qxCorrect)
        {
            if (IsOnLine(targ, hitX, hitX + this[hitX, dir.OnlyY] * dir.OnlyY))
            {
                result = hitX;
                return true;
            }
        }
        else if (qyCorrect)
        {
            if (IsOnLine(targ, hitY, hitY + this[hitY, dir.OnlyX] * dir.OnlyX))
            {
                result = hitY;
                return true;
            }
        }

        if (IsCorrect(end) && !this[end])
        {
            result = end;
            return true;
        }

        result = default;
        return false;
    }

    private bool IsOnLine(Vec2Int pos, Vec2Int start, Vec2Int end)
    {
        Vec2Int dir = end - start;
        Vec2Int norm = new Vec2Int(-dir.y, dir.x);
        return Vec2Int.Dot(pos - start, norm) == 0
            && Vec2Int.Dot(pos - start, dir) >= 0
            && Vec2Int.Dot(end - pos, dir) >= 0;
    }

    public override void Update()
    {
        Array.Fill(_cells, default);

        for (int y = 0; y < height; y++)
        {
            FillAxis(new Vec2Int(width - 1, y), Vec2Int.right);
            FillAxis(new Vec2Int(0, y), Vec2Int.left);
        }

        for (int x = 0; x < width; x++)
        {
            FillAxis(new Vec2Int(x, height - 1), Vec2Int.up);
            FillAxis(new Vec2Int(x, 0), Vec2Int.down);
        }

        for (int x = 0; x < width; x++)
        {
            FillDiag(new Vec2Int(x, height - 1), Vec2Int.upRight);
            FillDiag(new Vec2Int(x, height - 1), Vec2Int.upLeft);

            FillDiag(new Vec2Int(x, 0), Vec2Int.downLeft);
            FillDiag(new Vec2Int(x, 0), Vec2Int.downRight);
        }

        for (int y = 0; y < height; y++)
        {
            FillDiag(new Vec2Int(width - 1, y), Vec2Int.upRight);
            FillDiag(new Vec2Int(0, y), Vec2Int.upLeft);

            FillDiag(new Vec2Int(0, y), Vec2Int.downLeft);
            FillDiag(new Vec2Int(width - 1, y), Vec2Int.downRight);
        }
    }

    private void FillDiag(Vec2Int pos, Vec2Int dir)
    {
        int obsCount = 0;
        while (IsCorrect(pos))
        {
            if (this[pos])
            {
                obsCount = 0;
                pos -= dir;
                continue;
            }

            ref Cell cell = ref _cells[pos.y * width + pos.x];
            cell[dir] = ++obsCount;

            Vec2Int xCheck = pos + dir.OnlyX * cell[dir.OnlyX];
            Vec2Int yCheck = pos + dir.OnlyY * cell[dir.OnlyY];
            if ((IsCorrect(xCheck) && !this[xCheck]) || (IsCorrect(yCheck) && !this[yCheck]))
            {
                obsCount = 0;
            }
            else
            {
                (Forced forced, int count) = GetForced(pos, dir);
                if (count != 0)
                    obsCount = 0;
            }

            pos -= dir;
        }
    }

    private void FillAxis(Vec2Int pos, Vec2Int dir)
    {
        int obsCount = 0;
        while (IsCorrect(pos))
        {
            if (this[pos])
            {
                obsCount = 0;
                pos -= dir;
                continue;
            }

            ref Cell cell = ref _cells[pos.y * width + pos.x];
            cell[dir] = ++obsCount;

            (Forced forced, int count) = GetForced(pos, dir);
            if (count != 0)
                obsCount = 0;

            pos -= dir;
        }
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

    [System.Runtime.CompilerServices.InlineArray(8)]
    private struct Cell
    {
        private int dist;

        public int this[Vec2Int dir]
        {
            get
            {
                int ind = dir switch
                {
                    (0, -1) => 0,
                    (0, 1) => 1,
                    (-1, 0) => 2,
                    (1, 0) => 3,
                    (1, 1) => 4,
                    (1, -1) => 5,
                    (-1, -1) => 6,
                    (-1, 1) => 7,
                    _ => throw new Exception(),
                };

                return this[ind];
            }
            set
            {
                int ind = dir switch
                {
                    (0, -1) => 0,
                    (0, 1) => 1,
                    (-1, 0) => 2,
                    (1, 0) => 3,
                    (1, 1) => 4,
                    (1, -1) => 5,
                    (-1, -1) => 6,
                    (-1, 1) => 7,
                    _ => throw new Exception(),
                };

                this[ind] = value;
            }
        }
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

    public int this[Vec2Int pos, Vec2Int dir]
    {
        get => _cells[pos.y * width + pos.x][dir];
        set => _cells[pos.y * width + pos.x][dir] = value;
    }
}
