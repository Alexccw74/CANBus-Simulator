using Xunit;

namespace CANBusSimulator.ModbusSlave.Tests;

public class ModbusFloatCodecTests
{
    [Theory]
    [InlineData(1500.25f)]
    [InlineData(-40.0f)]
    [InlineData(0.0f)]
    [InlineData(3212.75f)]
    public void Encode_ThenDecode_RoundTripsExactly(float value)
    {
        foreach (var order in new[] { ModbusFloatWordOrder.HighWordFirst, ModbusFloatWordOrder.LowWordFirst })
        {
            (ushort first, ushort second) = ModbusFloatCodec.Encode(value, order);
            float decoded = ModbusFloatCodec.Decode(first, second, order);

            Assert.Equal(value, decoded);
        }
    }

    [Fact]
    public void Encode_HighWordFirst_PutsExponentSignBitsInFirstRegister()
    {
        // 1.0f = 0x3F800000
        (ushort first, ushort second) = ModbusFloatCodec.Encode(1.0f, ModbusFloatWordOrder.HighWordFirst);

        Assert.Equal(0x3F80, first);
        Assert.Equal(0x0000, second);
    }

    [Fact]
    public void Encode_LowWordFirst_SwapsWordOrderRelativeToHighWordFirst()
    {
        (ushort hiFirst, ushort hiSecond) = ModbusFloatCodec.Encode(1.0f, ModbusFloatWordOrder.HighWordFirst);
        (ushort loFirst, ushort loSecond) = ModbusFloatCodec.Encode(1.0f, ModbusFloatWordOrder.LowWordFirst);

        Assert.Equal(hiFirst, loSecond);
        Assert.Equal(hiSecond, loFirst);
    }
}
