using System.Text;

namespace OneBRC.Tests;

public class LineEnumeratorTests
{
    [Fact]
    public void EnumeratesLines()
    {
        var bytes = Encoding.UTF8.GetBytes(Data);
        var enumerator = new LineEnumerator(bytes);
        
        Assert.True(enumerator.MoveNext());
        Assert.Equal("Alaska;-40.0", Encoding.UTF8.GetString(enumerator.Current));
        Assert.True(enumerator.MoveNext());
        Assert.Equal("Berlin;11.1", Encoding.UTF8.GetString(enumerator.Current));
        Assert.False(enumerator.MoveNext());
        Assert.Equal("London;9.8", Encoding.UTF8.GetString(enumerator.Rest));
    }
    
    private const string Data = "Alaska;-40.0\nBerlin;11.1\nLondon;9.8";
}