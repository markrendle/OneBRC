using System.Diagnostics;
using OneBRC;

const int chunkSize = 1024 * 1024;

var processor = new Processor(chunkSize, Environment.ProcessorCount);

var stopwatch = Stopwatch.StartNew();

processor.Run(args);

stopwatch.Stop();

Console.WriteLine($"{stopwatch.Elapsed}");
