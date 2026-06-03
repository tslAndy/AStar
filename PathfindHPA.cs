using System.Numerics;

class PathfindHPA : Pathfinder
{
    private readonly List<Vertex>[] _chunks;
    private readonly Dictionary<Vertex, VNode> _field;
    private readonly HashSet<Vertex> _closed;
    private readonly Heap<Vertex> _heap;

    private readonly int chunkX,
        chunkY;

    public const int CHUNK_SIZE = 16;

    public PathfindHPA(int width, int height)
        : base(width, height)
    {
        if (width % CHUNK_SIZE != 0 || height % CHUNK_SIZE != 0)
            throw new Exception($"Map size should be multiple of {CHUNK_SIZE}");

        chunkX = width / CHUNK_SIZE;
        chunkY = height / CHUNK_SIZE;

        _chunks = new List<Vertex>[chunkX * chunkY];
        for (int i = 0; i < _chunks.Length; i++)
            _chunks[i] = new List<Vertex>();

        _field = new Dictionary<Vertex, VNode>();
        _closed = new HashSet<Vertex>();
        _heap = new Heap<Vertex>();
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        throw new NotImplementedException();
    }

    private Vertex GetVertex(Vec2Int pos)
    {
        int cx = pos.x / CHUNK_SIZE;
        int cy = pos.y / CHUNK_SIZE;

        List<Vertex> chunkVerts = _chunks[cy * chunkX + cx];
        foreach (Vertex temp in chunkVerts)
            if (temp.pos == pos)
                return temp;

        Vertex vertex = new Vertex(pos, new List<Edge>());
        chunkVerts.Add(vertex);
        return vertex;
    }

    public void SetupBridges()
    {
        List<Vec2Int> temp = new List<Vec2Int>();
        for (int cy = 0; cy < chunkY; cy++)
        {
            for (int cx = 1; cx < chunkX; cx++)
            {
                Vec2Int pos = new Vec2Int(cx * CHUNK_SIZE - 1, cy * CHUNK_SIZE);
                long borderA = GetBorderLine(pos, Vec2Int.up);
                long borderB = GetBorderLine(pos + Vec2Int.right, Vec2Int.up);
                long border = borderA | borderB;

                HandleBorder(border, pos, Vec2Int.up, temp);
                for (int i = 0; i < temp.Count; i++)
                    _chunks[cy * chunkX + cx].Add(GetVertex(temp[i]));
            }
        }

        for (int cy = 1; cy < chunkY; cy++)
        {
            for (int cx = 0; cx < chunkX; cx++)
            {
                Vec2Int pos = new Vec2Int(cx * CHUNK_SIZE, cy * CHUNK_SIZE - 1);
                long borderA = GetBorderLine(pos, Vec2Int.right);
                long borderB = GetBorderLine(pos + Vec2Int.up, Vec2Int.right);
                long border = borderA | borderB;

                HandleBorder(border, pos, Vec2Int.right, temp);
                for (int i = 0; i < temp.Count; i++)
                    _chunks[cy * chunkX + cx].Add(GetVertex(temp[i]));
            }
        }
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
            temp.Add(pos + dir * (offset + (n >> 1)));
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

    public Path GetPath(Vertex start, Vertex end)
    {
        _field.Clear();
        _closed.Clear();
        _heap.Clear();

        _field.Add(start, default);
        _heap.Add(start, 0);

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

                VNode temp = new VNode(node.cost + edge.path.length, vert, edge.path);
                if (_field.TryGetValue(edge.end, out VNode existing))
                {
                    if (temp.cost < existing.cost)
                    {
                        _field[edge.end] = temp;
                        _heap.Change(edge.end, temp.cost);
                    }
                }
                else
                {
                    _field.Add(edge.end, temp);
                    _heap.Add(edge.end, temp.cost);
                }
            }
        }

        if (!_closed.Contains(end))
            return default;

        List<Vec2Int> points = new List<Vec2Int>();
        while (end != start)
        {
            VNode node = _field[end];
            for (int i = 0; i < node.path.Count; i++)
                points.Add(node.path[i]);
            end = node.prev;
        }

        int length = 0;
        for (int i = 0; i < points.Count - 1; i++)
            length += GetCost(points[i], points[i + 1]);

        return new Path(points.ToArray(), length);
    }

    public record Vertex(Vec2Int pos, List<Edge> edges);

    public record struct Edge(Vertex end, Path path);

    record struct VNode(int cost, Vertex prev, Path path) // TODO: добавить gcost, fcost и вместо prev путь к вертексу
    {
        public static bool operator <(VNode left, VNode right) => left.cost < right.cost;

        public static bool operator >(VNode left, VNode right) => left.cost > right.cost;
    }
}
