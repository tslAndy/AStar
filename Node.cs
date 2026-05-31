record struct Node(int gCost, int hCost, Vec2Int prev)
{
    public int fCost => gCost + hCost;

    public static bool operator <(Node left, Node right) =>
        (left.fCost < right.fCost) || (left.fCost == right.fCost && left.hCost < right.hCost);

    public static bool operator >(Node left, Node right) =>
        (left.fCost > right.fCost) || (left.fCost == right.fCost && left.hCost > right.hCost);
}
