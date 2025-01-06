using System.Text;

namespace Shared.Networking;

public unsafe struct BufferReader
{
    private readonly byte[] _buffer;
    private int _position;
    
    public bool Error;

    public BufferReader(byte[] buffer)
    {
        _buffer = buffer;
        _position = 0;
    }

    public byte ReadByte()
    {
        if (_buffer.Length - _position < sizeof(byte))
        {
            Error = true;
            return 0;
        }

        var result = _buffer[_position];
        _position += sizeof(byte);
        return result;
    }

    public bool ReadBoolean()
    {
        if (_buffer.Length - _position < sizeof(bool))
        {
            Error = true;
            return false;
        }

        var result = _buffer[_position];
        _position += sizeof(bool);
        return result == 1;
    }

    public short ReadInt16()
    {
        if (_buffer.Length - _position < sizeof(short))
        {
            Error = true;
            return 0;
        }

        short result;
        fixed (byte* ptr = _buffer)
            result = *((short*)(ptr + _position));
        
        _position += sizeof(short);
        return result;
    }

    public int ReadInt32()
    {
        if (_buffer.Length - _position < sizeof(int))
        {
            Error = true;
            return 0;
        }

        int result;
        fixed (byte* ptr = _buffer)
            result = *((int*)(ptr + _position));

        _position += sizeof(int);
        return result;
    }

    public long ReadInt64()
    {
        if (_buffer.Length - _position < sizeof(long))
        {
            Error = true;
            return 0;
        }

        long result;
        fixed (byte* ptr = _buffer)
            result = *((long*)(ptr + _position));

        _position += sizeof(long);
        return result;
    }

    public ushort ReadUInt16()
    {
        if (_buffer.Length - _position < sizeof(ushort))
        {
            Error = true;
            return 0;
        }

        ushort result;
        fixed (byte* ptr = _buffer)
            result = *((ushort*)(ptr + _position));

        _position += sizeof(ushort);
        return result;
    }

    public uint ReadUInt32()
    {
        if (_buffer.Length - _position < sizeof(uint))
        {
            Error = true;
            return 0;
        }

        uint result;
        fixed (byte* ptr = _buffer)
            result = *((uint*)(ptr + _position));

        _position += sizeof(uint);
        return result;
    }

    public ulong ReadUInt64()
    {
        if (_buffer.Length - _position < sizeof(ulong))
        {
            Error = true;
            return 0;
        }

        ulong result;
        fixed (byte* ptr = _buffer)
            result = *((ulong*)(ptr + _position));

        _position += sizeof(ulong);
        return result;
    }

    public float ReadFloat()
    {
        if (_buffer.Length - _position < sizeof(float))
        {
            Error = true;
            return 0.0f;
        }

        float result;
        fixed (byte* ptr = _buffer)
            result = *((float*)(ptr + _position));

        _position += sizeof(float);
        return result;
    }

    public double ReadDouble()
    {
        if (_buffer.Length - _position < sizeof(double))
        {
            Error = true;
            return 0.0f;
        }

        double result;
        fixed (byte* ptr = _buffer)
            result = *((double*)(ptr + _position));

        _position += sizeof(double);
        return result;
    }

    public string ReadString()
    {
        var length = ReadInt32();
        if (_buffer.Length - _position < length)
        {
            Error = true;
            return string.Empty;
        }

        var result = Encoding.UTF8.GetString(_buffer, _position, length);
        _position += length;
        return result;
    }
}