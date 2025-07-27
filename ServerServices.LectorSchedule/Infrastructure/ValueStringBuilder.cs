using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ServerServices.LectorSchedule.Infrastructure;

//https://habr.com/ru/company/clrium/blog/420051/
internal ref struct ValueStringBuilder(Span<char> InitialBuffer)
{
    private char[]? _ArrayToReturnToPool = null;
    private Span<char> _Chars = InitialBuffer;
    private int _Pos = 0;

    public int Length
    {
        get => _Pos;
        set
        {
            var delta = value - _Pos;
            if (delta > 0)
                Append('\0', delta);
            else
                _Pos = value;
        }
    }

    public override string ToString()
    {
        var s = new string(_Chars[.._Pos]);
        Clear();
        return s;
    }

    public bool TryCopyTo(Span<char> destination, out int CharsWritten)
    {
        if (_Chars[.._Pos].TryCopyTo(destination))
        {
            CharsWritten = _Pos;
            Clear();
            return true;
        }

        CharsWritten = 0;
        Clear();
        return false;
    }

    public void Insert(int index, char value, int count)
    {
        if (_Pos > _Chars.Length - count) 
            Grow(count);

        var remaining = _Pos - index;
        _Chars.Slice(index, remaining).CopyTo(_Chars[(index + count)..]);
        _Chars.Slice(index, count).Fill(value);
        _Pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Append(char c)
    {
        var pos = _Pos;
        if (pos >= _Chars.Length)
            GrowAndAppend(c);
        else
        {
            _Chars[pos] = c;
            _Pos = pos + 1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder Append(Span<char> chars)
    {
        foreach (var t in chars)
            Append(t);

        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueStringBuilder Append(string s)
    {
        var pos = _Pos;
        if (s.Length == 1 && pos < _Chars.Length)
        {
            _Chars[pos] = s[0];
            _Pos        = pos + 1;
        }
        else
            AppendSlow(s);

        return this;
    }

    public ValueStringBuilder Append<T>(T value, ReadOnlySpan<char> format, IFormatProvider? provider = null) 
        where T : ISpanFormattable
    {
        var chars = _Chars[_Pos..];

        var success = value.TryFormat(chars, out var written, format, provider);

        return this;
    }

    private void AppendSlow(string s)
    {
        var pos = _Pos;
        if (pos > _Chars.Length - s.Length) 
            Grow(s.Length);

        s.AsSpan().CopyTo(_Chars[pos..]);
        _Pos += s.Length;
    }

    public void Append(char c, int count)
    {
        if (_Pos > _Chars.Length - count) 
            Grow(count);

        var dst = _Chars.Slice(_Pos, count);

        for (var i = 0; i < dst.Length; i++) 
            dst[i] = c;
        _Pos += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<char> AppendSpan(int length)
    {
        var orig_pos = _Pos;
        if (orig_pos > _Chars.Length - length) 
            Grow(length);

        _Pos = orig_pos + length;
        return _Chars.Slice(orig_pos, length);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowAndAppend(char c)
    {
        Grow(1);
        Append(c);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int RequiredAdditionalCapacity)
    {
        Debug.Assert(RequiredAdditionalCapacity > _Chars.Length - _Pos);

        var pool_array = ArrayPool<char>.Shared.Rent(Math.Max(_Pos + RequiredAdditionalCapacity, _Chars.Length * 2));

        _Chars.CopyTo(pool_array);

        var to_return = _ArrayToReturnToPool;
        _Chars = _ArrayToReturnToPool = pool_array;
        if (to_return is not null) 
            ArrayPool<char>.Shared.Return(to_return);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Clear()
    {
        var to_return = _ArrayToReturnToPool;
        this = default; 
        if (to_return is not null) ArrayPool<char>.Shared.Return(to_return);
    }

    public static implicit operator string(ValueStringBuilder s) => s.ToString();
}