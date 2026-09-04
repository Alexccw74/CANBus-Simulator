namespace CANBusSimulator.ModbusSlave;

/// <summary>
/// Which 16-bit word of a 32-bit float's two registers comes first. Different Modbus masters/PLCs
/// disagree on this, so it's a setting rather than hard-coded - a well-known Modbus interoperability gotcha.
/// </summary>
public enum ModbusFloatWordOrder
{
    HighWordFirst,
    LowWordFirst,
}

/// <summary>Configuration for <see cref="ModbusSlaveService"/>.</summary>
public sealed class ModbusSlaveOptions
{
    /// <summary>TCP port to listen on. 502 is the standard Modbus port but requires elevated/administrator
    /// privileges on Windows; a higher port (e.g. 5020) is a common choice for easy local testing.</summary>
    public int Port { get; init; } = 502;

    /// <summary>Modbus unit/slave id this service responds as.</summary>
    public byte UnitId { get; init; } = 1;

    /// <summary>First holding register address used by <see cref="RegisterMap"/>.</summary>
    public ushort RegisterBase { get; init; } = 0;

    public ModbusFloatWordOrder FloatWordOrder { get; init; } = ModbusFloatWordOrder.HighWordFirst;
}
