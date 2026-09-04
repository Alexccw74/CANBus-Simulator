namespace CANBusSimulator.Service;

/// <summary>
/// Bound from the "Modbus" configuration section - a plain settable POCO so configuration binding is
/// unambiguous, then mapped into a <see cref="ModbusSlave.ModbusSlaveOptions"/> for the slave service.
/// </summary>
public sealed class ModbusServiceOptions
{
    public int Port { get; set; } = 502;
    public byte UnitId { get; set; } = 1;
    public ushort RegisterBase { get; set; }

    /// <summary>"HighWordFirst" or "LowWordFirst" - see <see cref="ModbusSlave.ModbusFloatWordOrder"/>.</summary>
    public string FloatWordOrder { get; set; } = "HighWordFirst";
}
