using System;
using System.IO;
using System.Text;

namespace OneBRC;

public static class FileAnalyzer
{
    private const byte NewLine = (byte)'\n';
    
    public static long[] FindOffsets(string filePath)
    {
        var info = new FileInfo(filePath);
        var processorCount = Environment.ProcessorCount;

        var roughSize = info.Length / processorCount;
        var offsets = new long[processorCount];
        
        using var handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        Span<byte> buffer = stackalloc byte[128];

        long currentOffset = 0;

        for (int i = 0; i < processorCount; i++)
        {
            offsets[i] = currentOffset;

            if (i + 1 == processorCount) break;
            
            currentOffset += roughSize;
            
            int read = RandomAccess.Read(handle, buffer, currentOffset);

            var bytes = buffer[..read];

            // var all = Encoding.UTF8.GetString(bytes);

            int byteNewlineIndex = bytes.IndexOf(NewLine);

            if (byteNewlineIndex >= 0)
            {
                currentOffset += byteNewlineIndex + 1;
            }
            read = RandomAccess.Read(handle, buffer, currentOffset);
            bytes = buffer[..read];
            // all = Encoding.UTF8.GetString(bytes);
        }

        return offsets;
    }
}