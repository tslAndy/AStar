class PathfindSimple : Pathfinder
{
    public PathfindSimple(int width, int height)
        : base(width, height) { }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        Dictionary<Vec2Int, Vec2Int> closed = new Dictionary<Vec2Int, Vec2Int>();
        Dictionary<Vec2Int, Node> opened = new Dictionary<Vec2Int, Node>();

        opened.Add(start, new Node(0, GetCost(start, end), start));

        while (opened.Count != 0)
        {
            KeyValuePair<Vec2Int, Node> current = new KeyValuePair<Vec2Int, Node>(
                default,
                new Node(1_000_000, 1_000_000, default)
            );
            foreach (KeyValuePair<Vec2Int, Node> kvp in opened)
                if (kvp.Value < current.Value)
                    current = kvp;

            Vec2Int pos = current.Key;
            Node node = current.Value;

            closed.Add(pos, node.prev);
            opened.Remove(pos);

            if (pos == end)
                break;

            for (int dy = -1; dy < 2; dy++)
            {
                for (int dx = -1; dx < 2; dx++)
                {
                    if (dy == 0 && dx == 0)
                        continue;

                    Vec2Int nextPos = pos + new Vec2Int(dx, dy);
                    if (!IsCorrect(nextPos) || closed.ContainsKey(nextPos) || this[nextPos])
                        continue;

                    int deltaCost = (Math.Abs(dx) + Math.Abs(dy)) == 1 ? 10 : 14;

                    if (opened.TryGetValue(nextPos, out Node nextNode))
                    {
                        if (node.gCost + deltaCost < nextNode.gCost)
                        {
                            nextNode.prev = pos;
                            nextNode.gCost = node.gCost + deltaCost;
                            opened[nextPos] = nextNode;
                        }
                    }
                    else
                    {
                        nextNode = new Node(node.gCost + deltaCost, GetCost(nextPos, end), pos);
                        opened.Add(nextPos, nextNode);
                    }
                }
            }
        }

        if (!closed.ContainsKey(end))
            return new Path(null, opened.Keys.ToArray(), closed.Keys.ToArray());

        List<Vec2Int> path = new List<Vec2Int>();
        while (end != start)
        {
            path.Add(end);
            end = closed[end];
        }
        path.Add(start);

        return new Path(path.ToArray(), opened.Keys.ToArray(), closed.Keys.ToArray());
    }
}
