using System.Text;

namespace OneBRC.Tests;

public class FastParseTests
{
    [Theory]
    [InlineData("1", 1f)]
    [InlineData("42", 42f)]
    [InlineData("-42", -42f)]
    [InlineData("42.1", 42.1f)]
    [InlineData("42.235", 42.235f)]
    [InlineData("-42.235", -42.235f)]
    public void Parses(string input, float expected)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var actual = FastParse.FastParseFloat(bytes);
        Assert.Equal(expected, actual);
    }
}