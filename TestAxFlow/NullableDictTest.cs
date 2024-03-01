using AxFlow;

namespace TestAxFlow;

public class NullableDictTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestNullableDict()
    {
        var dict = new NullableDict<K?, V>();
        dict.Add(null, new V { Value = 0 });
        dict.Add(K.A, new V { Value = 1 });
        dict.Add(K.B, new V { Value = 2 });
        Assert.AreEqual(dict[null].Value, 0);
        Assert.AreEqual(dict[K.A].Value, 1);
        Assert.AreEqual(dict[K.B].Value, 2);
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
}