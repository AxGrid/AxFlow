using AxFlow;

namespace TestAxFlow;

public class NullableDictTest
{
    [SetUp]
    public void Setup()
    {
    }

   
    
    [Test]
    public void TestDictWithNull()
    {
        var dict = new Dictionary<NullObject<K?>, V>();
        Assert.IsFalse(dict.ContainsKey(null));
        dict.Add(null, new V { Value = 0 });
        Assert.IsTrue(dict.ContainsKey(null));
        dict.Add(K.A, new V { Value = 1 });
        dict.Add(K.B, new V { Value = 2 });
        Assert.That(dict[null].Value, Is.EqualTo(0));
        Assert.That(dict[K.A].Value, Is.EqualTo(1));
        Assert.That(dict[K.B].Value, Is.EqualTo(2));

        var dc = new GenTestDictClass<K, V>();
        var dict2 = dc.Dict;
        K? k = null; 
        Assert.IsFalse(dc.Dict.ContainsKey(k));
        dict2.Add(null, new V { Value = 0 });
        Assert.IsTrue(dict2.ContainsKey(null));
        dict2.Add(K.A, new V { Value = 1 });
        dict2.Add(K.B, new V { Value = 2 });
        Assert.That(dict2[null].Value, Is.EqualTo(0));
        Assert.That(dict2[K.A].Value, Is.EqualTo(1));
        Assert.That(dict2[K.B].Value, Is.EqualTo(2));
    }

    public enum K
    {
        A,
        B,
    }

    public class V
    {
        public int Value { get; set; }
    }

    public class GenTestDictClass<TKey, TValue> where TKey : struct
    {
        public Dictionary<NullObject<TKey?>, TValue> Dict { get; } = new();
    }
}