namespace OneBRC;

public struct ValueCounter
{
    private string _name;
    private int _count;
    private long _total;
    private long _min;
    private long _max;

    public void SetName(string name)
    {
        _name = name;
    }

    public void Record(long value)
    {
        if (_count == 0)
        {
            _min = _max = value;
        }
        else
        {
            if (value < _min) _min = value;
            if (value > _max) _max = value;
        }

        _total += value;
        ++_count;
    }

    public void Combine(ValueCounter other)
    {
        _count += other._count;
        _total += other._total;
        if (other._min < _min) _min = other._min;
        if (other._max < _max) _max = other._max;
        if (_name is null) _name = other._name;
    }

    public double Mean => ((double)_total / _count) / 1000;
    public double Min => (double)_min / 1000;
    public double Max => (double)_max / 1000;
    public int Count => _count;

    public string Name => _name;
}