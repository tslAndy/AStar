class PathfindHPA : Pathfinder
{
    public PathfindHPA(int width, int height)
        : base(width, height) { }

    public override Path GetPath(Vec2Int start, Vec2Int end)
    {
        throw new NotImplementedException();
    }

    private void Bake() { }

    public override bool this[Vec2Int pos]
    {
        get => base[pos];
        set
        {
            base[pos] = value;
            Bake();
        }
    }
}
