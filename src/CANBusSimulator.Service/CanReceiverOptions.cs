namespace CANBusSimulator.Service;

/// <summary>Bound from the "CanReceiver" configuration section.</summary>
public sealed class CanReceiverOptions
{
    /// <summary>"Slcan" (an SLCAN-speaking COM port - works today, including against the simulator
    /// over a virtual COM port pair) or "Advantech" (the PCM-9366's native driver - not yet
    /// implemented, see <see cref="Core.CanHardware.AdvantechCanReceiver"/>).</summary>
    public string Type { get; set; } = "Slcan";

    public string ComPort { get; set; } = "COM1";
    public int SerialBaudRate { get; set; } = 115200;
    public int CanBitrateBps { get; set; } = 250_000;

    public int AdvantechDeviceIndex { get; set; }
    public int AdvantechChannelIndex { get; set; }

    /// <summary>How long to wait before retrying after the receiver fails to open or faults.</summary>
    public int ReconnectDelaySeconds { get; set; } = 5;
}
