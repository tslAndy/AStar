class PathfindSimple : Pathfinder
{
    public PathfindSimple(int width, int height)
        : base(width, height) { }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        Dictionary<Vec2Int, Node> field = new Dictionary<Vec2Int, Node>();
        Dictionary<Vec2Int, Vec2Int> closed = new Dictionary<Vec2Int, Vec2Int>();
        Heap<Vec2Int> heap = new Heap<Vec2Int>();

        Node first = new Node(0, GetCost(start, end), start);
        field.Add(start, first);
        heap.Add(start, first.fCost);

        while (heap.Count != 0)
        {
            Vec2Int pos = heap.Pop();
            Node node = field[pos];

            closed.Add(pos, node.prev);
            field.Remove(pos);

            if (pos == end)
                break;

            for (int dy = -1; dy < 2; dy++)
            {
                for (int dx = -1; dx < 2; dx++)
                {
                    if (dy == 0 && dx == 0)
                        continue;

                    Vec2Int nextPos = pos + new Vec2Int(dx, dy);
                    if ((!IsCorrect(nextPos)) || closed.ContainsKey(nextPos) || this[nextPos])
                        continue;
                    field.Remove(pos);

                    int deltaCost = (Math.Abs(dx) + Math.Abs(dy)) == 1 ? 10 : 14;

                    if (field.TryGetValue(nextPos, out Node nextNode))
                    {
                        if (node.gCost + deltaCost < nextNode.gCost)
                        {
                            int oldCost = nextNode.fCost;

                            nextNode.prev = pos;
                            nextNode.gCost = node.gCost + deltaCost;

                            field[nextPos] = nextNode;
                            heap.Change(nextPos, nextNode.fCost, oldCost);
                        }
                    }
                    else
                    {
                        nextNode = new Node(node.gCost + deltaCost, GetCost(nextPos, end), pos);
                        field.Add(nextPos, nextNode);
                        heap.Add(nextPos, nextNode.fCost);
                    }
                }
            }
        }

        if (closed.ContainsKey(end))
            return BuildPath(closed, start, end);
        return default;
    }
}
