namespace CANBusSimulator.Core.J1939;

/// <summary>An immutable CAN frame ready for transmission.</summary>
public readonly record struct CanFrame(uint Id, bool Extended, byte Dlc, byte[] Data);
