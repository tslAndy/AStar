using System.Numerics;

class PathfindHPA : Pathfinder
{
    private readonly Dictionary<Vertex, VNode> _field;
    private readonly HashSet<Vertex> _closed;
    private readonly Heap<(int, int), Vertex> _heap;
    private readonly Dictionary<Vec2Int, State> _chunkState;

    private readonly Vec2Int _cdims;
    private readonly List<Vertex>[] _gates;
    private readonly Pathfinder _pathfinder;

    public const int CHUNK_SIZE = 16;

    public PathfindHPA(int width, int height)
        : base(width, height)
    {
        if (width % CHUNK_SIZE != 0 || height % CHUNK_SIZE != 0)
            throw new Exception($"Map size should be multiple of {CHUNK_SIZE}");

        _cdims = new Vec2Int(width / CHUNK_SIZE, height / CHUNK_SIZE);
        _pathfinder = new PathfindJps(CHUNK_SIZE, CHUNK_SIZE);
        _gates = new List<Vertex>[_cdims.y * _cdims.x];
        for (int i = 0; i < _gates.Length; i++)
            _gates[i] = new List<Vertex>();

        _field = new Dictionary<Vertex, VNode>();
        _closed = new HashSet<Vertex>();
        _heap = new Heap<(int, int), Vertex>(new ComparerAB());
        _chunkState = new Dictionary<Vec2Int, State>();

        // basic setup
        List<Vec2Int> temp = new List<Vec2Int>();

        for (int cy = 0; cy < _cdims.y; cy++)
        for (int cx = 1; cx < _cdims.x; cx++)
            UpdateBorder(
                new Vec2Int(cx, cy) * CHUNK_SIZE + Vec2Int.left,
                Vec2Int.up,
                Vec2Int.right,
                temp
            );

        for (int cy = 1; cy < _cdims.y; cy++)
        for (int cx = 0; cx < _cdims.x; cx++)
            UpdateBorder(
                new Vec2Int(cx, cy) * CHUNK_SIZE + Vec2Int.down,
                Vec2Int.right,
                Vec2Int.up,
                temp
            );

        for (int cy = 0; cy < _cdims.y; cy++)
        for (int cx = 0; cx < _cdims.x; cx++)
            AddInternal(new Vec2Int(cx, cy));
    }

    public override bool this[Vec2Int pos]
    {
        get => base[pos];
        set
        {
            base[pos] = value;
            bool isOnBorder =
                pos.x % CHUNK_SIZE == 0
                || pos.y % CHUNK_SIZE == 0
                || (pos.x + 1) % CHUNK_SIZE == 0
                || (pos.y + 1) % CHUNK_SIZE == 0;

            State flag = isOnBorder ? State.External : State.Internal;
            Vec2Int chunkPos = pos / CHUNK_SIZE;
            if (_chunkState.TryGetValue(chunkPos, out State state))
                _chunkState[chunkPos] = state | flag;
            else
                _chunkState[chunkPos] = flag;
        }
    }

    private bool CheckChunkPos(Vec2Int chunkPos) =>
        0 <= chunkPos.x && chunkPos.x < _cdims.x && 0 <= chunkPos.y && chunkPos.y < _cdims.y;

    private List<Vertex> GetChunk(Vec2Int chunkPos) => _gates[chunkPos.y * _cdims.x + chunkPos.x];

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        Vec2Int chunkPos = start / CHUNK_SIZE;
        Vec2Int offset = chunkPos * CHUNK_SIZE;

        if (chunkPos == end / CHUNK_SIZE)
        {
            FillPathfinder(offset);
            Path temp = GetPathLocal(start, end, offset);
            if (temp.Count != 0)
                return temp;
        }

        Vertex startVert = AddPointToChunk(start);
        Vertex endVert = AddPointToChunk(end);

        Path path = GetPath(startVert, endVert);

        RemovePointFromChunk(startVert);
        RemovePointFromChunk(endVert);

        return path;
    }

    private Path GetPath(Vertex start, Vertex end)
    {
        _field.Clear();
        _closed.Clear();
        _heap.Clear();

        VNode first = new VNode(0, GetCost(start.pos, end.pos), default, default);
        _field.Add(start, first);
        _heap.Add(start, (first.fCost, first.hCost));

        while (_heap.Count != 0)
        {
            Vertex vert = _heap.Pop();
            VNode node = _field[vert];

            _closed.Add(vert);

            if (vert == end)
                break;

            for (int i = 0; i < vert.edges.Count; i++)
            {
                Edge edge = vert.edges[i];
                if (_closed.Contains(edge.end))
                    continue;

                VNode temp = new VNode(
                    node.gcost + edge.path.length,
                    GetCost(edge.end.pos, end.pos),
                    vert,
                    edge.path
                );
                if (_field.TryGetValue(edge.end, out VNode existing))
                {
                    if (temp.fCost < existing.fCost)
                    {
                        _field[edge.end] = temp;
                        _heap.Change(edge.end, (temp.fCost, temp.hCost));
                    }
                }
                else
                {
                    _field.Add(edge.end, temp);
                    _heap.Add(edge.end, (temp.fCost, temp.hCost));
                }
            }
        }

        if (!_closed.Contains(end))
            return default;

        List<Vec2Int> points = new List<Vec2Int>();
        while (end != start)
        {
            VNode node = _field[end];
            for (int i = node.path.Count - 1; i >= 0; i--)
                points.Add(node.path[i]);
            end = node.prev;
        }

        Smooth(points);
        points.Reverse();

        int length = 0;
        for (int i = 0; i < points.Count - 1; i++)
            length += GetCost(points[i], points[i + 1]);

        return new Path(points.ToArray(), length);
    }

    // check if it's possible to remove two points
    // or use only one instead
    private void Smooth(List<Vec2Int> points)
    {
        int i = 0;
        while (i < points.Count - 3)
        {
            if (!CheckPath(points[i], points[i + 3], out Vec2Int? newPoint))
            {
                i++;
                continue;
            }

            points.RemoveAt(i + 2);
            if (newPoint != null)
                points[i + 1] = newPoint.Value;
            else
                points.RemoveAt(i + 1);
        }
    }

    private bool CheckPath(Vec2Int start, Vec2Int end, out Vec2Int? newPoint)
    {
        Vec2Int dir = (end - start).Clamp(-1, 1);
        Vec2Int delta = (end - start).Abs();

        if (delta.x == 0 || delta.y == 0 || delta.x == delta.y)
        {
            newPoint = null;
            return CheckDir(start, end, dir);
        }

        Vec2Int axDir = delta.x > delta.y ? dir.OnlyX : dir.OnlyY;

        int digSteps = Math.Min(delta.x, delta.y);
        int axSteps = Math.Max(delta.x, delta.y) - digSteps;

        Vec2Int newPointA = start + axSteps * axDir;
        if (CheckDir(start, newPointA, axDir) && CheckDir(newPointA, end, dir))
        {
            newPoint = newPointA;
            return true;
        }

        Vec2Int newPointB = start + digSteps * dir;
        if (CheckDir(start, newPointB, dir) && CheckDir(newPointB, end, axDir))
        {
            newPoint = newPointB;
            return true;
        }

        newPoint = null;
        return false;
    }

    private bool CheckDir(Vec2Int start, Vec2Int end, Vec2Int dir)
    {
        while (start != end)
        {
            if (this[start])
                return false;
            start += dir;
        }
        return true;
    }

    private Vertex AddPointToChunk(Vec2Int point)
    {
        Vec2Int chunkPos = point / CHUNK_SIZE;
        Vec2Int offset = chunkPos * CHUNK_SIZE;
        FillPathfinder(offset);

        Vertex start = new Vertex(point, new List<Edge>());
        List<Vertex> verts = GetChunk(chunkPos);
        for (int i = 0; i < verts.Count; i++)
        {
            Vertex end = verts[i];
            Path path = GetPathLocal(start.pos, end.pos, offset);
            if (path.Count == 0)
                continue;

            start.edges.Add(new Edge(end, path));
            end.edges.Add(new Edge(start, path with { reversed = true }));
        }

        return start;
    }

    private void RemovePointFromChunk(Vertex vertex)
    {
        Vec2Int chunkPos = vertex.pos / CHUNK_SIZE;
        for (int j = 0; j < vertex.edges.Count; j++)
            vertex.edges[j].end.edges.Delete(vertex, (edge, vertex) => edge.end == vertex);
        GetChunk(chunkPos).Remove(vertex);
    }

    public override void Update()
    {
        Vec2Int[] chunks = _chunkState.Keys.ToArray();

        foreach (Vec2Int chunkPos in chunks)
        {
            State chunkState = _chunkState[chunkPos];

            RemoveInternal(chunkPos);
            if ((chunkState & State.External) == 0)
                continue;

            Vec2Int onDir;
            if (CheckChunkPos(onDir = chunkPos + Vec2Int.up))
            {
                RemoveInternal(onDir);
                if (!_chunkState.ContainsKey(onDir))
                    _chunkState.Add(onDir, State.Internal);
            }

            if (CheckChunkPos(onDir = chunkPos + Vec2Int.down))
            {
                RemoveInternal(onDir);
                if (!_chunkState.ContainsKey(onDir))
                    _chunkState.Add(onDir, State.Internal);
            }

            if (CheckChunkPos(onDir = chunkPos + Vec2Int.left))
            {
                RemoveInternal(onDir);
                if (!_chunkState.ContainsKey(onDir))
                    _chunkState.Add(onDir, State.Internal);
            }

            if (CheckChunkPos(onDir = chunkPos + Vec2Int.right))
            {
                RemoveInternal(onDir);
                if (!_chunkState.ContainsKey(onDir))
                    _chunkState.Add(onDir, State.Internal);
            }
        }

        foreach (Vec2Int chunkPos in chunks)
        {
            State chunkState = _chunkState[chunkPos];
            if ((chunkState & State.External) != 0)
                RemoveExternal(chunkPos);
        }

        List<Vec2Int> temp = new List<Vec2Int>();
        foreach (Vec2Int chunkPos in chunks)
        {
            State chunkState = _chunkState[chunkPos];
            if ((chunkState & State.External) == 0)
                continue;

            _chunkState[chunkPos] = State.Updated;

            Vec2Int up = chunkPos + Vec2Int.up;
            if (CheckChunkPos(up) && (_chunkState[up] & State.Updated) == 0)
                UpdateBorder(up * CHUNK_SIZE + Vec2Int.down, Vec2Int.right, Vec2Int.up, temp);

            Vec2Int down = chunkPos + Vec2Int.down;
            if (CheckChunkPos(down) && (_chunkState[down] & State.Updated) == 0)
                UpdateBorder(chunkPos * CHUNK_SIZE + Vec2Int.down, Vec2Int.right, Vec2Int.up, temp);

            Vec2Int left = chunkPos + Vec2Int.left;
            if (CheckChunkPos(left) && (_chunkState[left] & State.Updated) == 0)
                UpdateBorder(chunkPos * CHUNK_SIZE + Vec2Int.left, Vec2Int.up, Vec2Int.right, temp);

            Vec2Int right = chunkPos + Vec2Int.right;
            if (CheckChunkPos(right) && (_chunkState[right] & State.Updated) == 0)
                UpdateBorder(right * CHUNK_SIZE + Vec2Int.left, Vec2Int.up, Vec2Int.right, temp);
        }

        foreach (Vec2Int chunkPos in _chunkState.Keys)
            AddInternal(chunkPos);
        _chunkState.Clear();
    }

    private void AddInternal(Vec2Int chunkPos)
    {
        Vec2Int offset = chunkPos * CHUNK_SIZE;
        FillPathfinder(offset);

        List<Vertex> verts = GetChunk(chunkPos);
        for (int i = 0; i < verts.Count - 1; i++)
        {
            Vertex vert_a = verts[i];

            for (int j = i + 1; j < verts.Count; j++)
            {
                Vertex vert_b = verts[j];

                Path path = GetPathLocal(vert_a.pos, vert_b.pos, offset);
                if (path.Count == 0)
                    continue;

                vert_a.edges.Add(new Edge(vert_b, path));
                vert_b.edges.Add(new Edge(vert_a, path with { reversed = true }));
            }
        }
    }

    private void RemoveInternal(Vec2Int chunkPos)
    {
        List<Vertex> verts = GetChunk(chunkPos);
        for (int i = 0; i < verts.Count; i++)
        {
            Vertex vert = verts[i];

            int k = 0;
            while (k < vert.edges.Count)
            {
                if (vert.edges[k].end.pos / CHUNK_SIZE == chunkPos)
                    vert.edges.RemoveAt(k);
                else
                    k++;
            }
        }
    }

    private void RemoveExternal(Vec2Int chunkPos)
    {
        List<Vertex> vertices = GetChunk(chunkPos);
        for (int i = 0; i < vertices.Count; i++)
        {
            Vertex vertex = vertices[i];
            for (int k = 0; k < vertex.edges.Count; k++)
            {
                Vertex endVertex = vertex.edges[k].end;
                endVertex.edges.Delete(vertex.pos, (edge, endPos) => edge.end.pos == endPos);
                if (endVertex.edges.Count == 0)
                    GetChunk(endVertex.pos / CHUNK_SIZE)
                        .Delete(endVertex.pos, (vert, pos) => vert.pos == pos);
            }
        }

        vertices.Clear();
    }

    private void FillPathfinder(Vec2Int offset)
    {
        for (int y = 0; y < CHUNK_SIZE; y++)
        {
            for (int x = 0; x < CHUNK_SIZE; x++)
            {
                Vec2Int pos = new Vec2Int(x, y);
                _pathfinder[pos] = this[pos + offset];
            }
        }
    }

    private Path GetPathLocal(Vec2Int start, Vec2Int end, Vec2Int offset)
    {
        start -= offset;
        end -= offset;

        Path path = _pathfinder.GetPath(start, end);
        for (int i = 0; i < path.Count; i++)
            path[i] += offset;
        return path;
    }

    private void UpdateBorder(Vec2Int pos, Vec2Int scanDir, Vec2Int offsetDir, List<Vec2Int> temp)
    {
        long border = GetBorderLine(pos, scanDir) | GetBorderLine(pos + offsetDir, scanDir);
        GetBorderPoints(border, pos, scanDir, temp);

        List<Vertex> leftChunk = GetChunk(pos / CHUNK_SIZE);
        List<Vertex> rightChunk = GetChunk(pos / CHUNK_SIZE + offsetDir);

        for (int i = 0; i < temp.Count; i++)
        {
            if (!leftChunk.TryGet(temp[i], (vert, pos) => vert.pos == pos, out Vertex leftVert))
            {
                leftVert = new Vertex(temp[i], new List<Edge>());
                leftChunk.Add(leftVert);
            }

            if (
                !rightChunk.TryGet(
                    temp[i] + offsetDir,
                    (vert, pos) => vert.pos == pos,
                    out Vertex rightVert
                )
            )
            {
                rightVert = new Vertex(temp[i] + offsetDir, new List<Edge>());
                rightChunk.Add(rightVert);
            }

            leftVert.edges.Add(new Edge(rightVert, new Path(null, 10)));
            rightVert.edges.Add(new Edge(leftVert, new Path(null, 10)));
        }
    }

    private void GetBorderPoints(long border, Vec2Int pos, Vec2Int dir, List<Vec2Int> temp)
    {
        temp.Clear();

        int offset = 0;
        while (border != -1L)
        {
            if ((border & 1) == 1)
            {
                border = (1 << 63) | (border >> 1);
                offset++;
                continue;
            }

            int n = BitOperations.TrailingZeroCount(border);
            border = ((1L << 63) >> (n - 1)) | (border >> n);

            if (n < 3)
                temp.Add(pos + dir * offset); // + (n >> 1)
            else
                for (int i = 1; i < n; i += CHUNK_SIZE >> 2)
                    temp.Add(pos + dir * (offset + i));

            offset += n;
        }
    }

    // I'm walking down the line
    // That divides me somewhere in my mind
    private long GetBorderLine(Vec2Int pos, Vec2Int dir)
    {
        long result = ~0;
        for (int i = 0; i < CHUNK_SIZE; i++)
        {
            if (!this[pos])
                result &= ~(1L << (i));
            pos += dir;
        }
        return result;
    } // Of the edge, and where I walk alone

    private record struct Vertex(Vec2Int pos, List<Edge> edges);

    private record struct Edge(Vertex end, Path path);

    private record struct VNode(int gcost, int hCost, Vertex prev, Path path)
    {
        public int fCost => gcost + hCost;
    }

    private enum State
    {
        Idle = 0,
        Internal = 1,
        External = 2,
        Updated = 8,
    }
}

static class ListExtensions
{
    public static bool TryGet<T, U>(this List<T> list, U param, Func<T, U, bool> func, out T elem)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (func(list[i], param))
            {
                elem = list[i];
                return true;
            }
        }

        elem = default;
        return false;
    }

    public static void Delete<T, U>(this List<T> list, U param, Func<T, U, bool> func)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (func(list[i], param))
            {
                list.RemoveAt(i);
                break;
            }
        }
    }
}
