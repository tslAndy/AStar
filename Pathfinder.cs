abstract class Pathfinder
{
    public readonly int width,
        height;
    private readonly long[] _field;

    protected Pathfinder(int width, int height)
    {
        this.width = width;
        this.height = height;

        int len = width * height;
        this._field = new long[len / 64 + ((len % 64) > 0 ? 1 : 0)];
    }

    public abstract Path GetPath(Vec2Int start, Vec2Int end);

    protected Path BuildPath(Dictionary<Vec2Int, Vec2Int> closed, Vec2Int start, Vec2Int end)
    {
        List<Vec2Int> path = new List<Vec2Int>();
        while (end != start)
        {
            path.Add(end);
            end = closed[end];
        }
        path.Add(start);

        int length = 0;
        for (int i = 0; i < path.Count - 1; i++)
            length += GetCost(path[i], path[i + 1]);

        return new Path(path.ToArray(), length);
    }

    protected int GetCost(Vec2Int start, Vec2Int end)
    {
        int dx = Math.Abs(end.x - start.x);
        int dy = Math.Abs(end.y - start.y);

        return 14 * Math.Min(dx, dy) + 10 * (Math.Max(dx, dy) - Math.Min(dx, dy));
    }

    protected bool IsCorrect(Vec2Int pos) =>
        0 <= pos.x && pos.x < width && 0 <= pos.y && pos.y < height;

    public virtual bool this[Vec2Int pos]
    {
        get
        {
            int ind = pos.y * width + pos.x;
            int div = ind >> 6;
            int rem = ind & 63;
            return (_field[div] & (1L << rem)) != 0;
        }
        set
        {
            int ind = pos.y * width + pos.x;
            int div = ind >> 6;
            int rem = ind & 63;
            _field[div] &= ~(1L << rem);
            _field[div] |= value ? (1L << rem) : 0;
        }
    }
}

struct Path
{
    public Vec2Int[] points;
    public int length;
    public bool reversed;

    public Path(Vec2Int[] points, int length)
    {
        this.points = points;
        this.length = length;
    }
}
