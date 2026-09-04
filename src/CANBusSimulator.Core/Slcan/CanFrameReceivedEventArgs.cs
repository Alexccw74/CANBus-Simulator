using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Slcan;

/// <summary>Raised by an <see cref="ICanReceiver"/> whenever a CAN frame has been parsed off the wire.</summary>
public sealed class CanFrameReceivedEventArgs : EventArgs
{
    public CanFrame Frame { get; }
    public DateTime Timestamp { get; }

    public CanFrameReceivedEventArgs(CanFrame frame, DateTime timestamp)
    {
        Frame = frame;
        Timestamp = timestamp;
    }
}
