namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Transport-agnostic contract for receiving CAN frames from a physical or virtual CAN interface.
/// Implemented by <see cref="SlcanReceiver"/> (an SLCAN-speaking COM port) and by
/// <see cref="CanHardware.AdvantechCanReceiver"/> (the native Advantech/SJA1000 driver), so the rest
/// of the pipeline (decoding, Modbus mapping) never needs to know which transport is in use.
/// </summary>
public interface ICanReceiver : IDisposable
{
    event EventHandler<CanFrameReceivedEventArgs>? FrameReceived;

    /// <summary>Raised when the underlying transport fails after having been opened (device unplugged, read error, etc.).</summary>
    event EventHandler<Exception>? Faulted;

    void Open(int canBitrateBps);
    void Close();
}
