using System.Buffers;
using System.Text;

namespace ServerServices.LectorSchedule.Infrastructure;

internal static class StringEx
{
    public static bool ContainsIgnoreCase(this string? str, string v) => str?.Contains(v, StringComparison.OrdinalIgnoreCase) ?? false;

    private static readonly System.Buffers.SearchValues<char> __WordSeparators = System.Buffers.SearchValues.Create([' ', '\u00a0', '\r', '\n', '\t', '.', ',', ':', ';', '"', '\'', '\\', '/', '[', ']', '(', ')', '{', '}', '|', '!', '?', '>', '<', '=', '+']);

    public static IEnumerable<Range> EnumWordsRanges(this string s) => s.AsMemory().EnumWordsRanges();

    public static IEnumerable<Range> EnumWordsRanges(this ReadOnlyMemory<char> s)
    {
        ArgumentNullException.ThrowIfNull(s);

        var length = s.Length;
        if (length == 0)
            yield break;

        var pos = 0;

        while (pos < length)
        {
            var index = s.Span[pos..].IndexOfAny(__WordSeparators);
            if (index == 0)
            {
                pos++;
                continue;
            }

            if (index < 0)
            {
                yield return new(pos, length);
                break;
            }

            yield return new(pos, index + pos);
            pos += index + 1;
        }
    }

    public static string ToUpperWords(this string Str)
    {
        var str = Str.AsSpan();

        var buffer_array = Str.Length > 128
            ? ArrayPool<char>.Shared.Rent(Str.Length)
            : null;
        var buffer = buffer_array is null
            ? stackalloc char[Str.Length]
            : buffer_array.AsSpan(0, Str.Length);

        try
        {
            var result = new StringBuilder(str.Length + 1);
            foreach (var word_range in Str.EnumWordsRanges())
            {
                var word = str[word_range];
                var first_char = word[0];
                var tail = word[1..];
                var buffer_len = tail.ToLowerInvariant(buffer);

                result.Append(first_char.ToUpperInvariant()).Append(buffer[..buffer_len]).Append(' ');
            }

            return result.ToString(0, result.Length - 1);
        }
        finally
        {
            if(buffer_array is not null)
                ArrayPool<char>.Shared.Return(buffer_array);
        }
    }

    //public static string TransliterateToRU(this string str)
    //{
    //    var source = str.AsSpan();

    //    var result = new StringBuilder(str.Length);

    //    while(source.Length > 0)
    //    {
    //        if(source.StartsWith())
    //        {

    //        }
    //    }
    //}
}

// а  - a
// б  - b
// ц  - c
// ч  - ch
// д  - d
// е  - e
// э  - e
// ф  - f
// г  - g
// х  - h
// и  - i
// ы  - i
// й  - j
// к  - k
// х  - kh
// л  - l
// м  - m
// н  - n
// о  - o
// п  - p
// ку - q
// р  - r
// с  - s
// щ  - sch
// щ  - shch
// ш  - sh
// т  - t
// ц  - tс
// у  - u
// в  - v
//    - w
// х  - x
// ы  - y
// я  - ya
// ю  - yu
// ё  - yo
// з  - z
// ж  - zh

// ъ - \"
// ь - '

