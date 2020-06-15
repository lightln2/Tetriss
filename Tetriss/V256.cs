using System;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

public readonly unsafe struct V256
{
    public static readonly V256 ZERO = 0;
    public static readonly V256 ONE = 1;
    public static readonly V256 V_255 = 0xFF;

    static V256() { if (!Avx2.IsSupported) throw new NotSupportedException("AVX2"); }

    private readonly Vector256<byte> _data;

    public V256(Vector256<byte> data) => _data = data;

    public V256(byte* b) { _data = Avx2.LoadVector256(b); }

    public V256(params byte[] buffer)
    {
        // todo: allocate new buffer on stack instead of resizing
        if (buffer.Length < 32) Array.Resize(ref buffer, 32);
        fixed (byte* p = buffer) _data = Avx2.LoadVector256(p);
    }

    public static V256 Load<T>(params T[] buffer) where T : unmanaged
    {
        fixed (T* p = buffer) return new V256((byte*)p);
    }

    public void Store<T>(T[] buffer) where T : unmanaged
    {
        fixed (T* p = buffer) Avx2.Store((byte*)p, _data);
    }

    public T[] ToArray<T>() where T : unmanaged
    {
        // todo: check sizeof(T) which should be 1, 2, 4, 8, 16, 32, 64, 128, or 256
        T[] array = new T[32 / sizeof(T)];
        fixed (T* ptr = array) Avx2.Store((byte*)ptr, _data);
        return array;
    }

    public byte this[int index]
    {
        get
        {
            byte* buffer = stackalloc byte[32];
            Avx2.Store(buffer, _data);
            return buffer[index];
        }
    }

    public V256 Shuffle(V256 mask) => Avx2.Shuffle(_data, mask._data);
    public V256 Min(V256 other) => Avx2.Min(_data, other._data);
    public V256 Max(V256 other) => Avx2.Max(_data, other._data);
    public V256 CompareEqual(V256 other) => Avx2.CompareEqual(_data, other._data);
    
    public static V256 FromByte(byte b) => Avx2.BroadcastScalarToVector256(&b);
    public static V256 FromSByte(sbyte b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromShort(short b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromUShort(ushort b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromInt(int b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromUInt(uint b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromLong(long b) => Avx2.BroadcastScalarToVector256(&b).AsByte();
    public static V256 FromULong(ulong b) => Avx2.BroadcastScalarToVector256(&b).AsByte();

    public static implicit operator V256(byte b) => FromByte(b);
    public static implicit operator V256(Vector256<byte> v) => new V256(v);
    public static implicit operator Vector256<byte>(V256 v) => v._data;

    public static V256 operator &(V256 x, V256 y) => Avx2.And(x._data, y._data);
    public static V256 operator |(V256 x, V256 y) => Avx2.Or(x._data, y._data);
    public static V256 operator ~(V256 x) => Avx2.AndNot(x._data, V_255._data);
    public static bool operator ==(V256 x, V256 y) => -1 == Avx2.MoveMask(Avx2.CompareEqual(x._data, y._data));
    public static bool operator !=(V256 x, V256 y) => -1 != Avx2.MoveMask(Avx2.CompareEqual(x._data, y._data));
    public static V256 operator +(V256 x, V256 y) => Avx2.Add(x._data, y._data);
    public static V256 operator -(V256 x, V256 y) => Avx2.Subtract(x._data, y._data);
    public static V256 operator -(V256 x) => Avx2.Subtract(ZERO._data, x._data);

    // count should be less than 64
    public static V256 operator >>(V256 x, int count)
    {
        Vector256<ulong> data = Avx2.ShiftRightLogical(x._data.AsUInt64(), (byte)count);
        Vector256<ulong> carry = Avx2.ShiftLeftLogical(x._data.AsUInt64(), (byte)(64 - count));
        carry = Avx2.Permute4x64(carry, 0x39);
        carry = Avx2.Blend(ZERO._data.AsUInt32(), carry.AsUInt32(), 0x3F).AsUInt64();
        data = Avx2.Or(data, carry);
        return data.AsByte();
    }

    // count should be less than 64
    public static V256 operator <<(V256 x, int count)
    {
        Vector256<ulong> data = Avx2.ShiftLeftLogical(x._data.AsUInt64(), (byte)count);
        Vector256<ulong> carry = Avx2.ShiftRightLogical(x._data.AsUInt64(), (byte)(64 - count));
        carry = Avx2.Permute4x64(carry, 0x93);
        carry = Avx2.Blend(ZERO._data.AsUInt32(), carry.AsUInt32(), 0xFC).AsUInt64();
        data = Avx2.Or(data, carry);
        return data.AsByte();
    }

    public override bool Equals(object obj) => this == (V256)obj;

    public override int GetHashCode()
    {
        byte* buffer = stackalloc byte[32];
        Avx2.Store(buffer, _data);
        ulong* ubuf = (ulong*)buffer;
        ulong longHC = ((ubuf[0] * 31 + ubuf[1]) * 31 + ubuf[2]) * 31 + ubuf[3];
        return (int)(longHC ^ (longHC >> 32));
    }

    public override string ToString()
    {
        byte* buffer = stackalloc byte[32];
        Avx2.Store(buffer, _data);
        return string.Join(" ", Enumerable.Range(0, 32).Select(i => buffer[i].ToString("X2")));
    }

}
