using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Shared.Networking;

public unsafe struct BufferWriter
{
    private byte[] _buffer;
    private int _position;

    public BufferWriter()
        : this(256)
    {
    }

    public BufferWriter(int initialCapacity)
    {
        _buffer = new byte[initialCapacity];
        _position = 4;
    }

    public void WriteByte(byte value)
    {
        Span<byte> span = GetSpan(sizeof(byte));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(byte);
    }

    public void WriteBoolean(bool value)
    {
        Span<byte> span = GetSpan(sizeof(bool));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value ? 1 : 0);

        _position += sizeof(bool);
    }

    public void WriteInt16(short value)
    {
        Span<byte> span = GetSpan(sizeof(short));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(short);
    }

    public void WriteInt32(int value)
    {
        Span<byte> span = GetSpan(sizeof(int));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(int);
    }

    public void WriteInt64(long value)
    {
        Span<byte> span = GetSpan(sizeof(long));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(long);
    }

    public void WriteUInt16(ushort value)
    {
        Span<byte> span = GetSpan(sizeof(ushort));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(ushort);
    }

    public void WriteUInt32(uint value)
    {
        Span<byte> span = GetSpan(sizeof(uint));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(uint);
    }

    public void WriteUInt64(ulong value)
    {
        Span<byte> span = GetSpan(sizeof(ulong));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(ulong);
    }

    public void WriteFloat(float value)
    {
        Span<byte> span = GetSpan(sizeof(float));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(float);
    }

    public void WriteDouble(double value)
    {
        Span<byte> span = GetSpan(sizeof(double));

        byte* ptr = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span));
        Unsafe.WriteUnaligned(ref *ptr, value);

        _position += sizeof(double);
    }

    public void WriteString(string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        WriteInt32(bytes.Length);

        Span<byte> span = GetSpan(bytes.Length);
        bytes.CopyTo(span);

        _position += bytes.Length + sizeof(int);
    }

    private Span<byte> GetSpan(int size)
    {
        EnsureCapacity(size);
        return _buffer.AsSpan().Slice(_position, size);
    }

    private void EnsureCapacity(int size)
    {
        if (_buffer.Length - _position >= size)
            return;

        var newCapacity = Math.Max(_buffer.Length * 2, _position + size);
        Array.Resize(ref _buffer, newCapacity);
    }

    public byte[] ToArray()
    {
        Unsafe.WriteUnaligned(ref _buffer[0], _position);
        return _buffer.AsSpan(0, _position).ToArray();
    }
}
