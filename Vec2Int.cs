record struct Vec2Int(int x, int y)
{
    public Vec2Int(int t)
        : this(t, t) { }

    public Vec2Int Clamp(int min, int max) =>
        new Vec2Int(Math.Clamp(this.x, min, max), Math.Clamp(this.y, min, max));

    public Vec2Int Abs() => new Vec2Int(Math.Abs(x), Math.Abs(y));

    public Vec2Int OnlyX => new Vec2Int(x, 0);
    public Vec2Int OnlyY => new Vec2Int(0, y);

    public static Vec2Int zero => new Vec2Int(0, 0);
    public static Vec2Int up => new Vec2Int(0, 1);
    public static Vec2Int down => new Vec2Int(0, -1);
    public static Vec2Int left => new Vec2Int(-1, 0);
    public static Vec2Int right => new Vec2Int(1, 0);
    public static Vec2Int upLeft => new Vec2Int(-1, 1);
    public static Vec2Int upRight => new Vec2Int(1, 1);
    public static Vec2Int downLeft => new Vec2Int(-1, -1);
    public static Vec2Int downRight => new Vec2Int(1, -1);

    public override string ToString() => $"({x} {y})";

    public static int Dot(Vec2Int left, Vec2Int right) => left.x * right.x + left.y * right.y;

    public static Vec2Int operator +(Vec2Int left) => new Vec2Int(left.x, left.y);

    public static Vec2Int operator -(Vec2Int left) => new Vec2Int(-left.x, -left.y);

    public static Vec2Int operator *(int k, Vec2Int vec) => new Vec2Int(vec.x * k, vec.y * k);

    public static Vec2Int operator *(Vec2Int vec, int k) => new Vec2Int(vec.x * k, vec.y * k);

    public static Vec2Int operator /(Vec2Int vec, int k) => new Vec2Int(vec.x / k, vec.y / k);

    public static Vec2Int operator *(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x * right.x, left.y * right.y);

    public static Vec2Int operator /(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x / right.x, left.y / right.y);

    public static Vec2Int operator +(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x + right.x, left.y + right.y);

    public static Vec2Int operator -(Vec2Int left, Vec2Int right) =>
        new Vec2Int(left.x - right.x, left.y - right.y);
}
