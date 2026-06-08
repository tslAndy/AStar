class Heap<TKey, TVal>
    where TVal : IEquatable<TVal>
{
    private readonly List<Node> _list;
    private readonly IComparer<TKey> _comparer;

    private const int DIMS = 4;
    private const int DIMS_POW = 2;

    private record struct Node(TVal elem, TKey prior);

    public Heap(IComparer<TKey> comparer, int capacity = 16)
    {
        _list = new List<Node>(capacity);
        _comparer = comparer;
    }

    public int Count => _list.Count;

    public void Clear() => _list.Clear();

    public bool Contains(TVal elem) => GetIndex(elem) >= 0;

    public TKey GetPriority(TVal elem) => _list[GetIndex(elem)].prior;

    public TVal PopAdd(TVal elem, TKey key)
    {
        TVal result = _list[0].elem;
        _list[0] = new Node(elem, key);
        SweepDown(0);
        return result;
    }

    public void Add(TVal elem, TKey key)
    {
        _list.Add(new Node(elem, key));
        SweepUp(_list.Count - 1);
    }

    public TVal Pop()
    {
        TVal val = _list[0].elem;
        _list[0] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        SweepDown(0);
        return val;
    }

    public (TVal, TKey) PopWithKey()
    {
        (TVal val, TKey key) = _list[0];
        _list[0] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        SweepDown(0);
        return (val, key);
    }

    public TVal Peek() => _list[0].elem;

    public (TVal, TKey) PeekWithKey() => (_list[0].elem, _list[0].prior);

    public void Change(TVal elem, TKey key)
    {
        int index = GetIndex(elem);
        if (index < 0)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));
        ChangeAt(index, key);
    }

    private void ChangeAt(int index, TKey newKey)
    {
        Node cur = _list[index];
        Node next = cur with { prior = newKey };

        _list[index] = next;

        int sign = _comparer.Compare(next.prior, cur.prior);
        if (sign < 0)
            SweepUp(index);
        else if (sign > 0)
            SweepDown(index);
    }

    public void Remove(TVal elem)
    {
        int index = GetIndex(elem);
        if (index < 0)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));
        RemoveAt(index);
    }

    private void RemoveAt(int index)
    {
        Node cur = _list[index];
        Node next = _list[^1];

        _list[index] = next;
        _list.RemoveAt(_list.Count - 1);

        int sign = _comparer.Compare(next.prior, cur.prior);
        if (sign < 0)
            SweepUp(index);
        else if (sign > 0)
            SweepDown(index);
    }

    private void SweepUp(int index)
    {
        while (index > 0)
        {
            Node child = _list[index];

            int parentInd = (index - 1) >> DIMS_POW;
            Node parent = _list[parentInd];
            if (_comparer.Compare(parent.prior, child.prior) <= 0)
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
            TKey childPrior = parent.prior;

            int firstChild = (index << DIMS_POW) + 1;
            int lastChild = Math.Min(firstChild + DIMS, _list.Count);
            for (int i = firstChild; i < lastChild; i++)
            {
                if (_comparer.Compare(_list[i].prior, childPrior) < 0)
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

    private int GetIndex(TVal elem)
    {
        int n = (_list.Count >> 1) + (_list.Count & 1);
        int end = _list.Count - 1;

        for (int i = 0; i < n; i++, end--)
        {
            if (elem.Equals(_list[i].elem))
                return i;
            if (elem.Equals(_list[end].elem))
                return end;
        }
        return -1;
    }

    public bool TryAdd(TVal elem, TKey key)
    {
        int index = GetIndex(elem);
        if (index >= 0)
            return false;

        Add(elem, key);
        return true;
    }

    public bool TryPop(out TVal elem)
    {
        if (_list.Count != 0)
        {
            elem = Pop();
            return true;
        }

        elem = default;
        return false;
    }

    public bool TryPopWithKey(out TVal elem, out TKey key)
    {
        if (_list.Count != 0)
        {
            (elem, key) = PopWithKey();
            return true;
        }

        elem = default;
        key = default;
        return false;
    }

    public bool TryPeek(out TVal elem)
    {
        if (_list.Count != 0)
        {
            elem = Peek();
            return true;
        }
        elem = default;
        return false;
    }

    public bool TryPeekWithKey(out TVal elem, out TKey key)
    {
        if (_list.Count != 0)
        {
            (elem, key) = PeekWithKey();
            return true;
        }
        elem = default;
        key = default;
        return false;
    }

    public bool TryChange(TVal elem, TKey key)
    {
        int index = GetIndex(elem);
        if (index < 0)
            return false;

        ChangeAt(index, key);
        return false;
    }

    public bool TryRemove(TVal elem)
    {
        int index = GetIndex(elem);
        if (index < 0)
            return false;
        Console.WriteLine($"{index} {_list.Count}");
        RemoveAt(index);
        return true;
    }
}
