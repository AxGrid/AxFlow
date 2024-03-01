using System.Collections.ObjectModel;

namespace AxFlow;

public class NullableDict<TK, TV> : IDictionary<TK, TV>
{
    Dictionary<TK, TV> _dict = new Dictionary<TK, TV>();
    TV _nullValue = default(TV);
    bool _hasNull = false;

    public NullableDict()
    {
    }

    public void Add(TK key, TV value)
    {
        if (key == null)
            if (_hasNull)
                throw new ArgumentException("Duplicate key");
            else
            {
                _nullValue = value;
                _hasNull = true;
            }
        else
            _dict.Add(key, value);
    }

    public bool ContainsKey(TK key)
    {
        if (key == null)
            return _hasNull;
        return _dict.ContainsKey(key);
    }

    public ICollection<TK> Keys
    {
        get
        {
            if (!_hasNull)
                return _dict.Keys;

            List<TK> keys = _dict.Keys.ToList();
            keys.Add(default(TK));
            return new ReadOnlyCollection<TK>(keys);
        }
    }

    public bool Remove(TK key)
    {
        if (key != null)
            return _dict.Remove(key);

        bool oldHasNull = _hasNull;
        _hasNull = false;
        return oldHasNull;
    }

    public bool TryGetValue(TK key, out TV value)
    {
        if (key != null)
            return _dict.TryGetValue(key, out value);

        value = _hasNull ? _nullValue : default(TV);
        return _hasNull;
    }

    public ICollection<TV> Values
    {
        get
        {
            if (!_hasNull)
                return _dict.Values;

            List<TV> values = _dict.Values.ToList();
            values.Add(_nullValue);
            return new ReadOnlyCollection<TV>(values);
        }
    }

    public TV this[TK key]
    {
        get
        {
            if (key == null)
                if (_hasNull)
                    return _nullValue;
                else
                    throw new KeyNotFoundException();
            else
                return _dict[key];
        }
        set
        {
            if (key == null)
            {
                _nullValue = value;
                _hasNull = true;
            }
            else
                _dict[key] = value;
        }
    }

    public void Add(KeyValuePair<TK, TV> item)
    {
        Add(item.Key, item.Value);
    }

    public void Clear()
    {
        _hasNull = false;
        _dict.Clear();
    }

    public bool Contains(KeyValuePair<TK, TV> item)
    {
        if (item.Key != null)
            return ((ICollection<KeyValuePair<TK, TV>>) _dict).Contains(item);
        if (_hasNull)
            return EqualityComparer<TV>.Default.Equals(_nullValue, item.Value);
        return false;
    }

    public void CopyTo(KeyValuePair<TK, TV>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<TK, TV>>) _dict).CopyTo(array, arrayIndex);
        if (_hasNull)
            array[arrayIndex + _dict.Count] = new KeyValuePair<TK, TV>(default(TK), _nullValue);
    }

    public int Count
    {
        get { return _dict.Count + (_hasNull ? 1 : 0); }
    }

    public bool IsReadOnly
    {
        get { return false; }
    }

    public bool Remove(KeyValuePair<TK, TV> item)
    {
        TV value;
        if (TryGetValue(item.Key, out value) && EqualityComparer<TV>.Default.Equals(item.Value, value))
            return Remove(item.Key);
        return false;
    }

    public IEnumerator<KeyValuePair<TK, TV>> GetEnumerator()
    {
        if (!_hasNull)
            return _dict.GetEnumerator();
        else
            return GetEnumeratorWithNull();
    }

    private IEnumerator<KeyValuePair<TK, TV>> GetEnumeratorWithNull()
    {
        yield return new KeyValuePair<TK, TV>(default(TK), _nullValue);
        foreach (var kv in _dict)
            yield return kv;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}