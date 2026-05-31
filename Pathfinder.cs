abstract class Pathfinder
{
    public readonly int width,
        height;
    private readonly bool[] _field;

    protected Pathfinder(int width, int height)
    {
        this.width = width;
        this.height = height;
        this._field = new bool[width * height];
    }

    public abstract Path GetPath(Vec2Int start, Vec2Int end);

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
        get => _field[pos.y * width + pos.x];
        set => _field[pos.y * width + pos.x] = value;
    }
}

class Path
{
    public Vec2Int[] points,
        opened,
        closed;

    public Path(Vec2Int[] points, Vec2Int[] opened, Vec2Int[] closed)
    {
        this.points = points;
        this.opened = opened;
        this.closed = closed;
    }
}
