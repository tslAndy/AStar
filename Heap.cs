class Heap<T>
    where T : IEquatable<T>
{
    private readonly List<Node> _list;
    private const int DIMS = 4;

    public Heap(int capacity = 16) => _list = new List<Node>(capacity);

    public int Count => _list.Count;

    public void Clear() => _list.Clear();

    public bool Contains(T elem) => IndexLinear(elem) >= 0;

    public int GetPriority(T elem) => _list[IndexLinear(elem)].prior;

    public T PopAdd(T elem, int prior)
    {
        T result = _list[0].elem;
        _list[0] = new Node(elem, prior);
        Update(0);
        return result;
    }

    public void Add(T elem, int prior)
    {
        _list.Add(new Node(elem, prior));
        Update(_list.Count - 1);
    }

    public T Pop()
    {
        T result = _list[0].elem;
        _list[0] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        Update(0);
        return result;
    }

    // slow change
    public void Change(T elem, int newPrior)
    {
        int index = IndexLinear(elem);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        Node node = _list[index];
        node.prior = newPrior;
        _list[index] = node;
        Update(index);
    }

    // fast change
    public void Change(T elem, int newPrior, int oldPrior)
    {
        IndexRecursive(elem, oldPrior, 0, out int index);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        Node node = _list[index];
        node.prior = newPrior;
        _list[index] = node;
        Update(index);
    }

    // slow remove, use when element priority is unknown
    public void Remove(T elem)
    {
        int index = IndexLinear(elem);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        _list[index] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        Update(index);
    }

    // faster remove, use if element priority is known
    public void Remove(T elem, int prior)
    {
        IndexRecursive(elem, prior, 0, out int index);
        if (index == -1)
            throw new ArgumentException($"Elem {elem} doesn't exist", nameof(elem));

        _list[index] = _list[^1];
        _list.RemoveAt(_list.Count - 1);
        Update(index);
    }

    private void Update(int index)
    {
        while (index > 0)
        {
            Node child = _list[index];

            int parentInd = (index - 1) / DIMS;
            Node parent = _list[parentInd];
            if (parent.prior <= child.prior)
                break;

            _list[index] = parent;
            _list[parentInd] = child;
            index = parentInd;
        }

        while (index < _list.Count)
        {
            Node parent = _list[index];

            int childInd = index;
            int childPrior = parent.prior;

            int firstChild = index * DIMS + 1;
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

    private bool IndexRecursive(T elem, int prior, int start, out int index)
    {
        if (start >= _list.Count || _list[start].prior > prior)
        {
            index = -1;
            return false;
        }

        if (_list[start].prior == prior && _list[start].elem.Equals(elem))
        {
            index = start;
            return true;
        }

        int firstChild = start * DIMS + 1;
        int lastChild = Math.Min(firstChild + DIMS, _list.Count);
        for (int i = firstChild; i < lastChild; i++)
        {
            if (IndexRecursive(elem, prior, i, out index))
                return true;
        }

        index = -1;
        return false;
    }

    private int IndexLinear(T elem)
    {
        for (int i = 0; i < _list.Count; i++)
            if (_list[i].elem.Equals(elem))
                return i;
        return -1;
    }

    private record struct Node(T elem, int prior);
}
