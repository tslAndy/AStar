record struct Node(int gCost, int hCost, Vec2Int prev)
{
    public int fCost => gCost + hCost;
}
