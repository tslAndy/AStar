using System.Numerics;

class PathfindHPA : Pathfinder
{
    private int chunkX,
        chunkY;
    public const int CHUNK_SIZE = 16;

    public PathfindHPA(int width, int height)
        : base(width, height)
    {
        chunkX = width / CHUNK_SIZE;
        chunkY = height / CHUNK_SIZE;

        if (width % CHUNK_SIZE != 0 || height % CHUNK_SIZE != 0)
            throw new Exception($"Map size should be multiple of {CHUNK_SIZE}");
    }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        throw new NotImplementedException();
    }

    public List<Vec2Int>[] GetBridges()
    {
        List<Vec2Int>[] chunks = new List<Vec2Int>[chunkX * chunkY];
        for (int i = 0; i < chunks.Length; i++)
            chunks[i] = new List<Vec2Int>();

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
                {
                    chunks[cy * chunkX + cx - 1].Add(temp[i]);
                    chunks[cy * chunkX + cx].Add(temp[i] + Vec2Int.right);
                }
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
                {
                    chunks[(cy - 1) * chunkX + cx].Add(temp[i]);
                    chunks[cy * chunkX + cx].Add(temp[i] + Vec2Int.up);
                }
            }
        }

        return chunks;
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

    public Vertex[] GetPath(Vertex start, Vertex end)
    {
        // здесь вместо предыдущего вертексу путь
        // хотя вертекс тоже нужен
        Dictionary<Vertex, Vertex> closed = new Dictionary<Vertex, Vertex>();
        Dictionary<Vertex, VNode> opened = new Dictionary<Vertex, VNode>();

        opened.Add(start, default);

        while (opened.Count != 0)
        {
            Vertex vert = null;
            VNode node = new VNode(1_000_000, null);
            foreach (KeyValuePair<Vertex, VNode> kvp in opened)
            {
                if (kvp.Value < node)
                {
                    vert = kvp.Key;
                    node = kvp.Value;
                }
            }

            closed.Add(vert, node.prev);
            opened.Remove(vert);

            if (vert == end)
                break;

            for (int i = 0; i < vert.edges.Count; i++)
            {
                Edge edge = vert.edges[i];
                if (closed.ContainsKey(edge.vert))
                    continue;

                VNode temp = new VNode(node.cost + edge.cost, vert);
                if (opened.TryGetValue(edge.vert, out VNode existing))
                {
                    if (temp.cost < existing.cost)
                        opened[edge.vert] = temp;
                }
                else
                {
                    opened.Add(edge.vert, temp);
                }
            }
        }

        if (!closed.ContainsKey(end))
            return null;

        List<Vertex> verts = new List<Vertex>();
        while (end != start)
        {
            verts.Add(end);
            end = closed[end];
        }
        verts.Add(start);
        return verts.ToArray();
    }

    public record Vertex(Vec2Int pos, List<Edge> edges);

    public record struct Edge(int cost, Vertex vert); // TODO: Сделать путь структурой, тогда его можно помечать как инвертный. Затем брать cost из path

    record struct VNode(int cost, Vertex prev) // TODO: добавить gcost, fcost и вместо prev путь к вертексу
    {
        public static bool operator <(VNode left, VNode right) => left.cost < right.cost;

        public static bool operator >(VNode left, VNode right) => left.cost > right.cost;
    }
}
