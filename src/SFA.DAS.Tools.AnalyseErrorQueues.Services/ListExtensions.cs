using System.Collections.Generic;
using System.Linq;

public static class ListExtensions
{
    public static IEnumerable<List<T>> ChunkBy<T>(this IEnumerable<T> source, int chunkSize) 
    {
        return source
            .Select((x, i) => new { Index = i, Value = x })
            .GroupBy(x => x.Index / chunkSize)
            .Select(x => x.Select(v => v.Value).ToList())
            .ToList();
    }

    public static string CrLfToTilde(this string textWithCrLf)
    {
        return textWithCrLf.Replace("\r\n", " ~ ").Replace("\r", " ~ ").Replace("\n", " ~ ");
    }
}