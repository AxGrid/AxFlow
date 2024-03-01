using System.ComponentModel;
using System.Reflection;

namespace AxFlow;

public static class Utils
{
    public static Dictionary<T1, T2> UnionDictionaries<T1, T2>(Dictionary<T1, T2> D2, Dictionary<T1, T2> D1)
    {
        Dictionary<T1, T2> rd = new Dictionary<T1, T2>(D1);
        foreach (var key in D2.Keys)
        {
            if (!rd.ContainsKey(key))
                rd.Add(key, D2[key]);
            else if(rd[key].GetType().IsGenericType)
            {
                if (rd[key].GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    var mBase = MethodBase.GetCurrentMethod();
                    MethodInfo info = mBase is MethodInfo ? (MethodInfo)mBase : typeof(Utils).GetMethod("UnionDictionaries", BindingFlags.Public | BindingFlags.Static);
                    var genericMethod = info.MakeGenericMethod(rd[key].GetType().GetGenericArguments()[0], rd[key].GetType().GetGenericArguments()[1]);
                    var invocationResult = genericMethod.Invoke(null, new object[] { rd[key], D2[key] });
                    rd[key] = (T2)invocationResult;
                }
            }
        }
        return rd;
    }
}

public struct NullObject<T>
{
    [DefaultValue(true)]
    private bool isnull;// default property initializers are not supported for structs

    private NullObject(T item, bool isnull) : this()
    {
        this.isnull = isnull;
        this.Item = item;
    }
      
    public NullObject(T item) : this(item, item == null)
    {
    }

    public static NullObject<T> Null()
    {
        return new NullObject<T>();
    }

    public T Item { get; private set; }

    public bool IsNull()
    {
        return this.isnull;
    }

    public static implicit operator T(NullObject<T> nullObject)
    {
        return nullObject.Item;
    }

    public static implicit operator NullObject<T>(T item)
    {
        return new NullObject<T>(item);
    }

    public override string ToString()
    {
        return (Item != null) ? Item.ToString() : "NULL";
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return this.IsNull();

        if (!(obj is NullObject<T>))
            return false;

        var no = (NullObject<T>)obj;

        if (this.IsNull())
            return no.IsNull();

        if (no.IsNull())
            return false;

        return this.Item.Equals(no.Item);
    }

    public override int GetHashCode()
    {
        if (this.isnull)
            return 0;

        var result = Item.GetHashCode();

        if (result >= 0)
            result++;

        return result;
    }
}


