namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Thin seam over a bidirectional byte stream used to talk to a real or virtual SLCAN serial port.
/// Mirrors <see cref="ISlcanTransport"/> but adds line-based reading, so <see cref="SlcanReceiver"/>'s
/// open/read/close sequence can be unit-tested without a real <see cref="System.IO.Ports.SerialPort"/>.
/// </summary>
public interface ISlcanLineSource : IDisposable
{
    bool IsOpen { get; }

    /// <summary>Raised once per complete line received (terminator stripped), e.g. "T18FEF90087D...".</summary>
    event EventHandler<string>? LineReceived;

    event EventHandler<Exception>? Faulted;

    void Open();
    void Write(string text);
    void Close();
}
