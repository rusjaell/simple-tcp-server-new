using Shared.Networking;

namespace UnitTests;

public sealed class BufferWriterReaderTests
{
    [Fact]
    public void BufferWriterReaderTest()
    {
        var writer = new NetworkBufferWriter();
        writer.WriteByte(15);
        writer.WriteBoolean(true);
        writer.WriteInt16(1523);
        writer.WriteInt32(589734534);
        writer.WriteInt64(8509437582347508723);
        writer.WriteUInt16(1523);
        writer.WriteUInt32(589734534);
        writer.WriteUInt64(8509437582347508723);
        writer.WriteFloat(5683534.23849723423f);
        writer.WriteDouble(34895734.2347234234234234223423);
        writer.WriteString("Hello, world!");

        var buffer = writer.ToArray();

        var reader = new BufferReader(buffer);

        var byteValue = reader.ReadByte();
        var boolValue = reader.ReadBoolean();
        var shortValue = reader.ReadInt16();
        var intValue = reader.ReadInt32();
        var longValue = reader.ReadInt64();
        var ushortValue = reader.ReadUInt16();
        var uintValue = reader.ReadUInt32();
        var ulongValue = reader.ReadUInt64();
        var floatValue = reader.ReadFloat();
        var doubleValue = reader.ReadDouble();
        var stringValue = reader.ReadString();

        Assert.Equal<int>(15, byteValue);
        Assert.True(boolValue);
        Assert.Equal<short>(1523, shortValue);
        Assert.Equal<int>(589734534, intValue);
        Assert.Equal<long>(8509437582347508723, longValue);
        Assert.Equal<ushort>(1523, ushortValue);
        Assert.Equal<uint>(589734534, uintValue);
        Assert.Equal<ulong>(8509437582347508723, ulongValue);
        Assert.Equal<float>(5683534.23849723423f, floatValue);
        Assert.Equal<double>(34895734.2347234234234234223423, doubleValue);
        Assert.Equal("Hello, world!", stringValue);
    }
}
