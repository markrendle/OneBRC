using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using OneBRC;

const byte SemiColon = (byte)';';
const int ChunkSize = 1024 * 1024;

var stopwatch = Stopwatch.StartNew();

var filePath = Path.GetFullPath(args[0]);

var offsets = FileAnalyzer.FindOffsets(filePath);
var splitReaders = new SplitReader[offsets.Length];

for (int i = 0; i < offsets.Length - 1; i++)
{
    splitReaders[i] = new SplitReader(filePath, ChunkSize, offsets[i], offsets[i + 1]);
}

splitReaders[^1] = new SplitReader(filePath, ChunkSize, offsets[^1], -1);

Parallel.ForEach(splitReaders, new ParallelOptions
{
    MaxDegreeOfParallelism = Environment.ProcessorCount
}, reader => reader.Run());

Console.WriteLine();
Console.WriteLine("Merging...");
Console.WriteLine();

var final = splitReaders[0].Dictionary;

for (int i = 1; i < splitReaders.Length; i++)
{
    var splitReader = splitReaders[i];
    foreach (var (key, counter) in splitReader.Dictionary)
    {
        // Console.WriteLine(key);
        ref var current = ref CollectionsMarshal.GetValueRefOrAddDefault(final, key, out _);
        current.Combine(counter);
    }

    // Console.Write("Enter to continue: ");
    // if (Console.ReadLine() is "q" or "Q") break;
}

Console.WriteLine();

var list = new SortedList<string, ValueCounter>();

foreach (var (key, counter) in final)
{
    list.Add(key, counter);
}

foreach (var (key, counter) in list)
{
    Console.WriteLine($"{key}={counter.Min:F1}/{counter.Mean:F1}/{counter.Max:F1}");
}

stopwatch.Stop();
Console.WriteLine(stopwatch.Elapsed);
