namespace CANBusSimulator.ModbusSlave;

/// <summary>Encodes/decodes a 32-bit IEEE-754 float across two 16-bit Modbus registers.</summary>
public static class ModbusFloatCodec
{
    public static (ushort First, ushort Second) Encode(float value, ModbusFloatWordOrder wordOrder)
    {
        uint bits = BitConverter.SingleToUInt32Bits(value);
        ushort high = (ushort)(bits >> 16);
        ushort low = (ushort)(bits & 0xFFFF);
        return wordOrder == ModbusFloatWordOrder.HighWordFirst ? (high, low) : (low, high);
    }

    public static float Decode(ushort first, ushort second, ModbusFloatWordOrder wordOrder)
    {
        (ushort high, ushort low) = wordOrder == ModbusFloatWordOrder.HighWordFirst
            ? (first, second)
            : (second, first);
        uint bits = ((uint)high << 16) | low;
        return BitConverter.UInt32BitsToSingle(bits);
    }
}
