using CANBusSimulator.Core.Domain;

namespace CANBusSimulator.Core.J1939;

/// <summary>
/// One parameter's decoded reading from a received CAN frame. <see cref="Value"/> is <see langword="null"/>
/// when the raw wire value was the SAE "not available"/"error" sentinel (mirrors what
/// <see cref="J1939FrameBuilder"/> reserves when encoding).
/// </summary>
public sealed record DecodedValue(SimulatedParameter Parameter, double? Value, byte SourceAddress, DateTime Timestamp);
