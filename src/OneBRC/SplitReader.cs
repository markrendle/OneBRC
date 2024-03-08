using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OneBRC;

public class SplitReader
{
    private static int _read = 0;
    
    private const byte Semicolon = (byte)';';
    
    private readonly Dictionary<long, string> _keys = new();
    private readonly Dictionary<string, ValueCounter> _dictionary = new();
    private readonly string _filePath;
    private readonly int _chunkSize;
    private readonly long _startPosition;
    private readonly long _endPosition;
    private readonly byte[] _buffer1, _buffer2;
    private readonly char[] _chars;

    public SplitReader(string filePath, int chunkSize, long startPosition, long endPosition)
    {
        _filePath = filePath;
        _chunkSize = chunkSize;
        _startPosition = startPosition;
        _endPosition = endPosition;
        _buffer1 = new byte[chunkSize];
        _buffer2 = new byte[chunkSize];
        _chars = new char[chunkSize];
    }

    public void Run()
    {
        var buffer = _buffer1;
        var spare = _buffer2;
        int writeOffset = 0;
        
        using var handle = File.OpenHandle(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        var endPosition = _endPosition > 0 ? _endPosition : RandomAccess.GetLength(handle);
        
        long offset = _startPosition;
        string after = "";
        while (offset < endPosition)
        {
            var remaining = (int)(endPosition - offset);
            if (writeOffset > 0)
            {
                
            }
            var span = remaining <= _chunkSize
                ? buffer.AsSpan()[writeOffset..remaining]
                : buffer.AsSpan()[writeOffset..];
            var read = RandomAccess.Read(handle, span, offset);
            span = buffer.AsSpan(0, writeOffset + read);
            // var before = after;
            // if (before.StartsWith("Ljubljana;22.598"))
            // {
            //     Debugger.Break();
            // }
            // var length = Encoding.UTF8.GetChars(span, _chars);
            // after = new string(_chars.AsSpan()[..length]);

            // var debug = Encoding.UTF8.GetString(span[..40]);
            // Console.WriteLine($"Run: {debug}");

            var enumerator = new LineEnumerator();

            while (enumerator.MoveNext())
            {
                ParseLine(enumerator.Current);
                // int lineCount = Interlocked.Increment(ref _read);
                // if (lineCount % 1000000 == 0)
                // {
                //     Console.WriteLine($"{lineCount}");
                // }
            }

            if ((writeOffset = enumerator.Rest.Length) > 0)
            {
                enumerator.Rest.CopyTo(spare);
            }
            (buffer, spare) = (spare, buffer);
            offset += _chunkSize;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ParseLine(ReadOnlySpan<byte> line)
    {
        var span = line;
        int scIndex = span.IndexOf(Semicolon);
        if (scIndex < 0) return;
        if (!float.TryParse(span[(scIndex + 1)..], out float value)) return;
        var key = line[..scIndex];
        var longKey = LongKey(key);

        ref string? name = ref CollectionsMarshal.GetValueRefOrAddDefault(_keys, longKey, out _);

        if (name is null)
        {
            name = Encoding.UTF8.GetString(key);
        }
        
        if (name.Contains(';'))
        {
            Console.Out.WriteLine(Encoding.UTF8.GetString(line));
        }
        ref ValueCounter counter = ref CollectionsMarshal.GetValueRefOrAddDefault(_dictionary, name, out _);
        counter.Count(value);
    }

    private static long LongKey(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 8)
        {
            return MemoryMarshal.Read<long>(bytes);
        }

        Span<byte> span = stackalloc byte[8];
        bytes.CopyTo(span);
        return MemoryMarshal.Read<long>(span);
    }

    public Dictionary<string, ValueCounter> Dictionary => _dictionary;
}

public class ReadOnlyMemoryByteComparer : EqualityComparer<ReadOnlyMemory<byte>>
{
    public static EqualityComparer<ReadOnlyMemory<byte>> Instance = new ReadOnlyMemoryByteComparer();

    private ReadOnlyMemoryByteComparer()
    {
        
    }

    public override bool Equals(ReadOnlyMemory<byte> x, ReadOnlyMemory<byte> y)
    {
        if (x.Span.SequenceEqual(y.Span)) return true;
        return false;
    }

    public override int GetHashCode(ReadOnlyMemory<byte> obj)
    {
        var span = obj.Span;

        if (span.Length == 0) return 0;
        
        int hashCode = span[0];
        while (span.Length > 4)
        {
            int next = MemoryMarshal.Read<int>(span[..4]);
            hashCode = HashCode.Combine(hashCode, next);
            span = span[4..];
        }

        switch (span.Length)
        {
            case 1:
                return HashCode.Combine(hashCode, span[0]);
            case 2:
                return HashCode.Combine(hashCode, MemoryMarshal.Read<short>(span));
            case 3:
                return HashCode.Combine(hashCode, MemoryMarshal.Read<short>(span), span[2]);
            default:
                return hashCode;
        }
    }
}