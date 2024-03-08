using GenerateData;

if (args.Length != 2)
{
    Console.Error.WriteLine("Usage: dotnet run -c Release -- 1000000000 ../../billion.txt");
}

int rows;
if (!int.TryParse(args[0], out rows))
{
    rows = 100;
}

var path = Path.GetFullPath(args[1]);

using var writer = File.CreateText(path);

int count = 0;

foreach (var station in Stations.Randomize(rows))
{
    if (++count % 1000000 == 0)
    {
        Console.WriteLine($"{count}...");
    }
    float value = Random.Shared.Next(100) - 50f;
    float point = Random.Shared.Next(1000);
    float actual = value + (point / 1000f);
    writer.Write($"{station};{actual}\n");
}

Console.WriteLine("Done.");
