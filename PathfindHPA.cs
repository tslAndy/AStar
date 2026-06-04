using System.Numerics;

class PathfindHPA : Pathfinder
{
    private readonly Dictionary<Vertex, VNode> _field;
    private readonly HashSet<Vertex> _closed;
    private readonly Heap<Vertex> _heap;

    private readonly Vec2Int _cdims;
    private readonly List<Vertex>[] _gates;
    private readonly Pathfinder _pathfinder;

    public const int CHUNK_SIZE = 8;

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
        _heap = new Heap<Vertex>();
    }

    public override void Update()
    {
        for (int i = 0; i < _gates.Length; i++)
            _gates[i].Clear();

        // thought also about saving vertices to object pool and reuse them again
        // but too lazy to implement it
        // so each time new are created

        FindBridges();
        ConnectBridges();
    }

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
        _heap.Add(start, first.fCost);

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
                        _heap.Change(edge.end, temp.fCost);
                    }
                }
                else
                {
                    _field.Add(edge.end, temp);
                    _heap.Add(edge.end, temp.fCost);
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
        List<Vertex> verts = GetGates(chunkPos);
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
        GetGates(chunkPos).Remove(vertex);
    }

    private Vertex GetVertex(Vec2Int pos)
    {
        Vec2Int chunkPos = pos / CHUNK_SIZE;
        List<Vertex> verts = GetGates(chunkPos);
        if (verts.TryGet(pos, (vert, pos) => vert.pos == pos, out Vertex vertex))
            return vertex;

        vertex = new Vertex(pos, new List<Edge>());
        verts.Add(vertex);
        return vertex;
    }

    private void FindBridges()
    {
        List<Vec2Int> temp = new List<Vec2Int>();

        for (int cy = 0; cy < _cdims.y; cy++)
        {
            for (int cx = 1; cx < _cdims.x; cx++)
            {
                Vec2Int pos = new Vec2Int(cx * CHUNK_SIZE - 1, cy * CHUNK_SIZE);
                long borderA = GetBorderLine(pos, Vec2Int.up);
                long borderB = GetBorderLine(pos + Vec2Int.right, Vec2Int.up);
                long border = borderA | borderB;

                HandleBorder(border, pos, Vec2Int.up, temp);
                List<Vertex> verts = GetGates(new Vec2Int(cx, cy));
                for (int i = 0; i < temp.Count; i++)
                    verts.Add(GetVertex(temp[i]));
            }
        }

        for (int cy = 1; cy < _cdims.y; cy++)
        {
            for (int cx = 0; cx < _cdims.x; cx++)
            {
                Vec2Int pos = new Vec2Int(cx * CHUNK_SIZE, cy * CHUNK_SIZE - 1);
                long borderA = GetBorderLine(pos, Vec2Int.right);
                long borderB = GetBorderLine(pos + Vec2Int.up, Vec2Int.right);
                long border = borderA | borderB;

                HandleBorder(border, pos, Vec2Int.right, temp);
                List<Vertex> verts = GetGates(new Vec2Int(cx, cy));
                for (int i = 0; i < temp.Count; i++)
                    verts.Add(GetVertex(temp[i]));
            }
        }
    }

    private void ConnectBridges()
    {
        for (int cy = 0; cy < _cdims.y; cy++)
        for (int cx = 0; cx < _cdims.x; cx++)
            ConnectChunk(new Vec2Int(cx, cy));
    }

    private void ConnectChunk(Vec2Int chunkPos)
    {
        Vec2Int offset = chunkPos * CHUNK_SIZE;
        FillPathfinder(offset);

        List<Vertex> verts = GetGates(chunkPos);
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

    private List<Vertex> GetGates(Vec2Int chunkPos) => _gates[chunkPos.y * _cdims.x + chunkPos.x];

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
        start = (start - offset).BoundMin(Vec2Int.zero);
        end = (end - offset).BoundMin(Vec2Int.zero);

        Path path = _pathfinder.GetPath(start, end);
        for (int i = 0; i < path.Count; i++)
            path[i] += offset;
        return path;
    }

    private void HandleBorder(long border, Vec2Int pos, Vec2Int dir, List<Vec2Int> temp)
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
                temp.Add(pos + dir * (offset)); // + (n >> 1)
            else
                for (int i = 1; i < n; i += 3)
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
