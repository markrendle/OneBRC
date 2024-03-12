using System.IO.MemoryMappedFiles;
using System.Text;

namespace OneBRC;

public static class MemoryMappedFileAnalyzer
{
    private const byte NewLine = (byte)'\n';
    private const int Megabyte = 1024 * 1024;

    public static unsafe MemoryMappedFileOffset[] GetOffsets(MemoryMappedFile mmf, long size, int threadCount)
    {
        using var accessor = mmf.CreateViewAccessor(0, size, MemoryMappedFileAccess.Read);
        byte* pointer = null;
        accessor.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);

        try
        {
            var offsets = new List<MemoryMappedFileOffset>();
            var roughChunkSize = size / threadCount;

            var over = roughChunkSize % Megabyte;
        
            var actualChunkSize = (roughChunkSize - over) + Megabyte;

            long offset = 0;
        
            while (offset + actualChunkSize < size)
            {
                var span = new Span<byte>(pointer + offset, 1024);
                int newline = span.IndexOf(NewLine);
                offsets.Add(new MemoryMappedFileOffset(offset, actualChunkSize + newline));
                offset += actualChunkSize + newline + 1;
            }
        
            offsets.Add(new MemoryMappedFileOffset(offset, size - offset));

            return offsets.ToArray();
        }
        finally
        {
            accessor.SafeMemoryMappedViewHandle.ReleasePointer();
        }
    }
    
    public static long[] FindOffsets(string filePath, int threadCount)
    {
        var info = new FileInfo(filePath);

        var roughSize = info.Length / threadCount;
        var offsets = new long[threadCount];
        
        using var handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        Span<byte> buffer = stackalloc byte[128];

        long currentOffset = 0;

        for (int i = 0; i < threadCount; i++)
        {
            offsets[i] = currentOffset;

            if (i + 1 == threadCount) break;
            
            currentOffset += roughSize;
            
            int read = RandomAccess.Read(handle, buffer, currentOffset);

            var bytes = buffer[..read];

            int byteNewlineIndex = bytes.IndexOf(NewLine);

            if (byteNewlineIndex >= 0)
            {
                currentOffset += byteNewlineIndex + 1;
            }
#if(DEBUG)
            read = RandomAccess.Read(handle, buffer, currentOffset);
            bytes = buffer[..read];
            var line = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(line.Split('\n').First());
#endif
        }

        return offsets;
    }
}

public readonly struct MemoryMappedFileOffset
{
    public readonly long Offset;
    public readonly long Length;

    public MemoryMappedFileOffset(long offset, long length)
    {
        Offset = offset;
        Length = length;
    }
}