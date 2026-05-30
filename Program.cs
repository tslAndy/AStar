// 1) A*
// 2) JPS
// 3) Regions
// 4) Regioins + JPS

using System.Numerics;
using Raylib_cs;

class Program
{
    public static void Main()
    {
        int width = 80;
        int height = 40;
        int cSize = 20;

        Pathfinder pathfinder = new Pathfinder(width, height);

        Vec2Int? start = null,
            end = null;

        Path? path = null;

        Raylib.InitWindow(width * cSize, height * cSize, "A*");

        while (!Raylib.WindowShouldClose())
        {
            Vector2 pos = Raylib.GetMousePosition() / cSize;
            Vec2Int posInt = new Vec2Int((int)pos.X, (int)pos.Y);

            if (Raylib.IsMouseButtonDown(MouseButton.Left))
            {
                pathfinder[posInt] = true;
                if (start != null && end != null)
                    path = pathfinder.GetPath(start.Value, end.Value);
            }
            else if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                pathfinder[posInt] = false;
                if (start != null && end != null)
                    path = pathfinder.GetPath(start.Value, end.Value);
            }
            else if (Raylib.IsMouseButtonPressed(MouseButton.Middle))
            {
                if (start == null)
                {
                    start = posInt;
                }
                else if (end == null)
                {
                    end = posInt;
                    path = pathfinder.GetPath(start.Value, end.Value);
                }
                else
                {
                    end = null;
                    start = null;
                    path = null;
                }
            }

            Raylib.BeginDrawing();
            Raylib.ClearBackground(Color.Black);

            if (start != null)
                Raylib.DrawRectangle(
                    start.Value.x * cSize,
                    start.Value.y * cSize,
                    cSize,
                    cSize,
                    Color.Yellow
                );

            if (end != null)
                Raylib.DrawRectangle(
                    end.Value.x * cSize,
                    end.Value.y * cSize,
                    cSize,
                    cSize,
                    Color.Yellow
                );
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (pathfinder[new Vec2Int(x, y)])
                        Raylib.DrawRectangle(x * cSize, y * cSize, cSize, cSize, Color.DarkBlue);
                }
            }

            if (path != null)
            {
                foreach (Vec2Int vec in path.opened)
                    Raylib.DrawRectangle(vec.x * cSize, vec.y * cSize, cSize, cSize, Color.Red);
                foreach (Vec2Int vec in path.closed)
                    Raylib.DrawRectangle(vec.x * cSize, vec.y * cSize, cSize, cSize, Color.White);

                foreach (Vec2Int vec in path.points)
                    Raylib.DrawRectangle(vec.x * cSize, vec.y * cSize, cSize, cSize, Color.Yellow);
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Raylib.DrawRectangleLines(x * cSize, y * cSize, cSize, cSize, Color.Gray);
                }
            }
            Raylib.EndDrawing();
        }
    }
}

class Path
{
    public Vec2Int[] points,
        closed,
        opened;

    public Path(Vec2Int[] points, Vec2Int[] closed, Vec2Int[] opened)
    {
        this.points = points;
        this.closed = closed;
        this.opened = opened;
    }
}

class Pathfinder
{
    public readonly int width,
        height;
    private readonly bool[] _field;

    public Pathfinder(int width, int height)
    {
        this.width = width;
        this.height = height;
        this._field = new bool[width * height];
    }

    public Path GetPath(Vec2Int start, Vec2Int end)
    {
        Dictionary<Vec2Int, Node> closed = new Dictionary<Vec2Int, Node>();
        Dictionary<Vec2Int, Node> opened = new Dictionary<Vec2Int, Node>();

        opened.Add(start, new Node(0, GetCostH(start, end), default));

        while (opened.Count != 0)
        {
            KeyValuePair<Vec2Int, Node> current = new KeyValuePair<Vec2Int, Node>(
                default,
                new Node(1_000_000, 1_000_000, default)
            );
            foreach (KeyValuePair<Vec2Int, Node> kvp in opened)
                if (
                    kvp.Value.fCost < current.Value.fCost
                    || (
                        kvp.Value.fCost == current.Value.fCost
                        && kvp.Value.hCost < current.Value.hCost
                    )
                )
                    current = kvp;

            closed.Add(current.Key, current.Value);
            opened.Remove(current.Key);

            if (current.Key == end)
                break;

            for (int dy = -1; dy < 2; dy++)
            {
                for (int dx = -1; dx < 2; dx++)
                {
                    if (dy == 0 && dx == 0)
                        continue;

                    Vec2Int nextPos = current.Key + new Vec2Int(dx, dy);

                    if (nextPos.x < 0 || nextPos.x >= width || nextPos.y < 0 || nextPos.y >= height)
                        continue;

                    if (closed.ContainsKey(nextPos) || this[nextPos])
                        continue;

                    int deltaCost = (Math.Abs(dx) + Math.Abs(dy)) == 1 ? 10 : 14;

                    if (opened.TryGetValue(nextPos, out Node nextNode))
                    {
                        if (current.Value.gCost + deltaCost < nextNode.gCost)
                        {
                            nextNode.prev = current.Key;
                            nextNode.gCost = current.Value.gCost + deltaCost;
                            opened[nextPos] = nextNode;
                        }
                    }
                    else
                    {
                        nextNode = new Node(
                            current.Value.gCost + deltaCost,
                            GetCostH(nextPos, end),
                            current.Key
                        );
                        opened.Add(nextPos, nextNode);
                    }
                }
            }
        }

        if (!closed.TryGetValue(end, out Node temp))
            return null;

        List<Vec2Int> path = new List<Vec2Int>();

        Vec2Int tempPos = end;
        while (tempPos != start)
        {
            path.Add(tempPos);
            tempPos = closed[tempPos].prev;
        }
        path.Add(start);

        return new Path(path.ToArray(), closed.Keys.ToArray(), opened.Keys.ToArray());
    }

    private int GetCostH(Vec2Int start, Vec2Int end)
    {
        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);

        return 14 * Math.Min(dx, dy) + 10 * (Math.Max(dx, dy) - Math.Min(dx, dy));
    }

    public bool this[Vec2Int pos]
    {
        get => _field[pos.y * width + pos.x];
        set => _field[pos.y * width + pos.x] = value;
    }
}

record struct Node(int gCost, int hCost, Vec2Int prev)
{
    public int fCost => gCost + hCost;
}

record struct Vec2Int(int x, int y)
{
    public static Vec2Int operator +(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x + right.x, left.y + right.y);

    public static Vec2Int operator -(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x - right.x, left.y - right.y);
}
