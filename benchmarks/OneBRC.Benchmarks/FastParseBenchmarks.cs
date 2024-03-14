using System.Text;
using BenchmarkDotNet.Attributes;

namespace OneBRC.Benchmarks;

public class FastParseBenchmarks
{
    private static readonly List<byte[]> Data = new();

    [GlobalSetup]
    public void GenerateData()
    {
        for (int i = 0; i < 1024; i++)
        {
            var value = (Random.Shared.NextSingle() - 0.5f) * 50f;
            Data.Add(Encoding.UTF8.GetBytes(value.ToString("F3")));
        }
    }

    [Benchmark(Baseline = true)]
    public double ParseFloat()
    {
        double result = 0d;
        var data = Data;
        foreach (var line in data)
        {
            result += FastParse.FastParseFloat(line);
        }

        return result;
    }

    [Benchmark]
    public long ParseLong()
    {
        long result = 0L;
        var data = Data;
        foreach (var line in data)
        {
            result += FastParse.FastParseLongFromFloat(line);
        }

        return result;
    }
}