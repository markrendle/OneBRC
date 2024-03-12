using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Text;
using OneBRC;

// const int chunkSize = 1024 * 1024;

// var processor = new Processor(chunkSize, Environment.ProcessorCount);

var mmfs = new MemoryMappedFileStrategy(Environment.ProcessorCount);
var filePath = Path.GetFullPath(args[0]);

var stopwatch = Stopwatch.StartNew();

mmfs.Play(filePath);
// processor.Run(args);

stopwatch.Stop();

Console.WriteLine($"{stopwatch.Elapsed}");
