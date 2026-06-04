class Heap<T>
    where T : IEquatable<T>
{
    private readonly List<Node> _list;

    private const int DIMS = 4;
    private const int DIMS_POW = 2;

    public Heap(int capacity = 16) => _list = new List<Node>(capacity);

    public int Count => _list.Count;

    public void Clear() => _list.Clear();

    public bool Contains(T elem) => GetIndex(elem) >= 0;

    public int GetPriority(T elem) => _list[GetIndex(elem)].prior;

    public T PopAdd(T elem, int prior)
    {
        T result = _list[0].elem;
        _list[0] = new Node(elem, prior);
        SweepDown(0);
        return result;
    }

    public void Add(T elem, int prior)
    {
        _list.Add(new Node(elem, prior));
        SweepUp(_list.Count - 1);
    }

    public T Pop()
    {
        T result = _list[0].elem;
        _list[0] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        SweepDown(0);
        return result;
    }

    public void Change(T elem, int newPrior)
    {
        int index = GetIndex(elem);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        Node cur = _list[index];
        Node next = cur with { prior = newPrior };

        _list[index] = next;

        if (next.prior < cur.prior)
            SweepUp(index);
        else if (next.prior > cur.prior)
            SweepDown(index);
    }

    public void Remove(T elem)
    {
        int index = GetIndex(elem);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        Node cur = _list[index];
        Node next = _list[^1];

        _list.RemoveAt(_list.Count - 1);
        _list[index] = next;

        if (next.prior < cur.prior)
            SweepUp(index);
        else if (next.prior > cur.prior)
            SweepDown(index);
    }

    private void SweepUp(int index)
    {
        while (index > 0)
        {
            Node child = _list[index];

            int parentInd = (index - 1) >> DIMS_POW;
            Node parent = _list[parentInd];
            if (parent.prior <= child.prior)
                break;

            _list[index] = parent;
            _list[parentInd] = child;
            index = parentInd;
        }
    }

    private void SweepDown(int index)
    {
        while (index < _list.Count)
        {
            Node parent = _list[index];

            int childInd = index;
            int childPrior = parent.prior;

            int firstChild = (index << DIMS_POW) + 1;
            int lastChild = Math.Min(firstChild + DIMS, _list.Count);
            for (int i = firstChild; i < lastChild; i++)
            {
                if (_list[i].prior < childPrior)
                {
                    childInd = i;
                    childPrior = _list[i].prior;
                }
            }

            if (childInd == index)
                break;

            _list[index] = _list[childInd];
            _list[childInd] = parent;
            index = childInd;
        }
    }

    private int GetIndex(T elem)
    {
        // TODO: remove
        EqualityComparer<T> eq = EqualityComparer<T>.Default;
        for (int i = 0; i < _list.Count; i++)
        {
            if (eq.Equals(elem, _list[i].elem))
                return i;
        }
        return -1;
        // int n = (_list.Count >> 1) + (_list.Count & 1);
        // int end = _list.Count - 1;
        //
        // for (int i = 0; i < n; i++, end--)
        // {
        //     if (elem.Equals(_list[i].elem))
        //         return i;
        //     if (elem.Equals(_list[end].elem))
        //         return end;
        // }
        // return -1;
    }

    private record struct Node(T elem, int prior);
}
