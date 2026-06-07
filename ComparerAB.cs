class ComparerAB : IComparer<(int, int)>
{
    public int Compare((int, int) x, (int, int) y) =>
        x.Item1 == y.Item1 ? x.Item2.CompareTo(y.Item2) : x.Item1.CompareTo(y.Item1);
}
