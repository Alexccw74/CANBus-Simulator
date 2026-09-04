using CANBusSimulator.Core.Domain;

namespace CANBusSimulator.ModbusSlave;

/// <summary>
/// Fixed holding-register layout, relative to a configurable base address:
///
///   base + 0            : heartbeat (rolling counter, increments on every decoded frame)
///   base + 1            : link status (0 = no frame received yet, 1 = at least one received)
///   base + 2 + 3n       : parameter n status (0 = stale/never seen, 1 = valid, 2 = SAE "not available")
///   base + 3 + 3n       : parameter n value, register 1 (word order per <see cref="ModbusSlaveOptions.FloatWordOrder"/>)
///   base + 4 + 3n       : parameter n value, register 2
///
/// where n is the index of the parameter within <see cref="ParameterCatalog.All"/>'s declared order
/// (Engine Speed, Coolant Temp, Oil Pressure, Fuel Level, Battery Voltage, Boost Pressure).
/// </summary>
public static class RegisterMap
{
    private const int RegistersPerParameter = 3;

    public const ushort ParameterStatusStale = 0;
    public const ushort ParameterStatusValid = 1;
    public const ushort ParameterStatusNotAvailable = 2;

    public static ushort HeartbeatAddress(ushort registerBase) => registerBase;

    public static ushort LinkStatusAddress(ushort registerBase) => (ushort)(registerBase + 1);

    public static ushort ParameterStatusAddress(ushort registerBase, int parameterIndex) =>
        (ushort)(registerBase + 2 + parameterIndex * RegistersPerParameter);

    public static ushort ParameterValueAddress(ushort registerBase, int parameterIndex) =>
        (ushort)(ParameterStatusAddress(registerBase, parameterIndex) + 1);

    /// <summary>The index of <paramref name="parameter"/> within <see cref="ParameterCatalog.All"/>.</summary>
    public static int IndexOf(SimulatedParameter parameter)
    {
        var all = ParameterCatalog.All;
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i].Key == parameter.Key)
                return i;
        }
        throw new ArgumentException($"'{parameter.Key}' is not in ParameterCatalog.All.", nameof(parameter));
    }
}
