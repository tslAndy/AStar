class PathfindSimple : Pathfinder
{
    private readonly Dictionary<Vec2Int, Node> _field;
    private readonly HashSet<Vec2Int> _closed;
    private readonly Heap<(int, int), Vec2Int> _heap;

    public PathfindSimple(int width, int height)
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

        while (_heap.TryPop(out Vec2Int pos))
        {
            Node node = _field[pos];

            _closed.Add(pos);

            if (pos == end)
                break;

            for (int dy = -1; dy < 2; dy++)
            {
                for (int dx = -1; dx < 2; dx++)
                {
                    if (dy == 0 && dx == 0)
                        continue;

                    Vec2Int nextPos = pos + new Vec2Int(dx, dy);
                    if ((!IsCorrect(nextPos)) || _closed.Contains(nextPos) || this[nextPos])
                        continue;

                    int deltaCost = (Math.Abs(dx) + Math.Abs(dy)) == 1 ? 10 : 14;

                    if (_field.TryGetValue(nextPos, out Node nextNode))
                    {
                        if (node.gCost + deltaCost < nextNode.gCost)
                        {
                            nextNode.prev = pos;
                            nextNode.gCost = node.gCost + deltaCost;

                            _field[nextPos] = nextNode;
                            _heap.Change(nextPos, (nextNode.fCost, nextNode.hCost));
                        }
                    }
                    else
                    {
                        nextNode = new Node(node.gCost + deltaCost, GetCost(nextPos, end), pos);
                        _field.Add(nextPos, nextNode);
                        _heap.Add(nextPos, (nextNode.fCost, nextNode.hCost));
                    }
                }
            }
        }

        if (_closed.Contains(end))
            return BuildPath(_field, start, end);
        return default;
    }
}
