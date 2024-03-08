using System.Text;

namespace OneBRC.Tests;

public class LineReaderTests
{
    [Fact]
    public void ReadsLines()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Data));
        var target = new LineReader(stream);

        AssertNext(ref target, "Alaska;-40.0");
        AssertNext(ref target, "Berlin;11.1");
        AssertNext(ref target, "London;9.8");
        Assert.False(target.MoveNext());
    }

    private static void AssertNext(ref LineReader target, string expected)
    {
        Assert.True(target.MoveNext());
        var actual = Encoding.UTF8.GetString(target.Current.Span);
        Assert.Equal(expected, actual);
    }
    
    private const string Data = """
                                Alaska;-40.0
                                Berlin;11.1
                                London;9.8
                                """;
}