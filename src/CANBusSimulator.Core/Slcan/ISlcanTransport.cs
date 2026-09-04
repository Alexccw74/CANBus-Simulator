namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Thin seam over the byte stream used to talk to a real or virtual serial port. Lets
/// <see cref="SlcanPort"/>'s open/write/close sequence be unit-tested without a real
/// <see cref="System.IO.Ports.SerialPort"/> or physical hardware.
/// </summary>
public interface ISlcanTransport : IDisposable
{
    bool IsOpen { get; }
    void Open();
    void Write(string text);
    void Close();
}
