using System.Runtime.InteropServices;

namespace OneBRC.Tests;

public class ValueCounterTests
{
    [Fact]
    public void WorksWithCollectionsMarshal()
    {
        var dict = new Dictionary<ReadOnlyMemory<byte>, ValueCounter>();
        ReadOnlyMemory<byte> key = "London"u8.ToArray();

        ref var counter = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
        counter.Count(1);
        
        ref var counter2 = ref CollectionsMarshal.GetValueRefOrAddDefault(dict, key, out _);
        counter2.Count(100);
        
        Assert.True(dict.TryGetValue(key, out counter));
        Assert.Equal(1, counter.Min);
        Assert.Equal(100, counter.Max);
    }
}