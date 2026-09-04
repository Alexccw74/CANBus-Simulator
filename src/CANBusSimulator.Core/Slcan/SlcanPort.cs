using System.IO.Ports;
using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Owns a serial transport and speaks the SLCAN/Lawicel ASCII protocol over it: sets the CAN bitrate
/// and opens the channel on <see cref="Open"/>, sends extended CAN frames, and closes the channel on
/// <see cref="Close"/>/<see cref="Dispose"/>. Real transport errors (port in use, device unplugged,
/// write timeout) are translated into <see cref="SlcanException"/> with a user-facing message.
/// </summary>
public sealed class SlcanPort : IDisposable
{
    private readonly ISlcanTransport _transport;

    /// <summary>Creates a port using a real <see cref="SerialPort"/> transport.</summary>
    public SlcanPort(string portName, int serialBaudRate)
        : this(new SerialPortTransport(portName, serialBaudRate))
    {
    }

    /// <summary>Creates a port over a caller-supplied transport (used by tests to avoid real hardware).</summary>
    public SlcanPort(ISlcanTransport transport)
    {
        _transport = transport;
    }

    public bool IsOpen => _transport.IsOpen;

    /// <summary>Enumerates COM ports currently available on this machine.</summary>
    public static string[] GetAvailablePorts() => SerialPort.GetPortNames();

    /// <summary>Opens the underlying transport and sends the SLCAN bitrate + open-channel commands.</summary>
    public void Open(int canBitrateBps)
    {
        try
        {
            _transport.Open();
            SendRaw(SlcanEncoder.EncodeSetBitrate(BaudRateMap.CodeFor(canBitrateBps)));
            SendRaw(SlcanEncoder.OpenChannel);
        }
        catch (Exception ex) when (ex is not SlcanException)
        {
            throw Translate(ex);
        }
    }

    /// <summary>Encodes and transmits a single CAN frame.</summary>
    public void SendFrame(CanFrame frame)
    {
        try
        {
            SendRaw(SlcanEncoder.EncodeExtendedFrame(frame));
        }
        catch (Exception ex) when (ex is not SlcanException)
        {
            throw Translate(ex);
        }
    }

    /// <summary>Sends the SLCAN close-channel command (best-effort) and closes the transport.</summary>
    public void Close()
    {
        try
        {
            if (_transport.IsOpen)
                SendRaw(SlcanEncoder.CloseChannel);
        }
        catch
        {
            // Best-effort: still close the transport below even if the final command failed.
        }
        finally
        {
            _transport.Close();
        }
    }

    public void Dispose()
    {
        Close();
        _transport.Dispose();
    }

    private void SendRaw(string text)
    {
        if (!_transport.IsOpen)
            throw new SlcanException("The COM port is not open.");
        _transport.Write(text);
    }

    private static SlcanException Translate(Exception ex) => ex switch
    {
        UnauthorizedAccessException => new SlcanException(
            "The COM port is already in use by another application.", ex),
        TimeoutException => new SlcanException(
            "Writing to the COM port timed out. The device may be unresponsive.", ex),
        IOException => new SlcanException(
            "The device appears to have been disconnected.", ex),
        _ => new SlcanException($"COM port error: {ex.Message}", ex),
    };
}
