using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Receives CAN frames from an SLCAN-speaking serial port: configures the CAN bitrate and opens the
/// channel on <see cref="Open"/> (same handshake as <see cref="SlcanPort"/>'s transmit side), parses
/// every incoming line via <see cref="SlcanFrameParser"/>, and raises <see cref="FrameReceived"/>.
/// </summary>
public sealed class SlcanReceiver : ICanReceiver
{
    private readonly ISlcanLineSource _lineSource;

    public event EventHandler<CanFrameReceivedEventArgs>? FrameReceived;
    public event EventHandler<Exception>? Faulted;

    /// <summary>Creates a receiver using a real serial port.</summary>
    public SlcanReceiver(string portName, int serialBaudRate)
        : this(new SerialPortLineSource(portName, serialBaudRate))
    {
    }

    /// <summary>Creates a receiver over a caller-supplied line source (used by tests to avoid real hardware).</summary>
    public SlcanReceiver(ISlcanLineSource lineSource)
    {
        _lineSource = lineSource;
        _lineSource.LineReceived += OnLineReceived;
        _lineSource.Faulted += OnLineSourceFaulted;
    }

    public void Open(int canBitrateBps)
    {
        try
        {
            _lineSource.Open();
            _lineSource.Write(SlcanEncoder.EncodeSetBitrate(BaudRateMap.CodeFor(canBitrateBps)));
            _lineSource.Write(SlcanEncoder.OpenChannel);
        }
        catch (Exception ex) when (ex is not SlcanException)
        {
            throw new SlcanException($"Could not open CAN receiver: {ex.Message}", ex);
        }
    }

    public void Close()
    {
        try
        {
            if (_lineSource.IsOpen)
                _lineSource.Write(SlcanEncoder.CloseChannel);
        }
        catch
        {
            // Best-effort: still close the line source below even if the final command failed.
        }
        finally
        {
            _lineSource.Close();
        }
    }

    private void OnLineReceived(object? sender, string line)
    {
        if (SlcanFrameParser.TryParse(line, out CanFrame frame))
            FrameReceived?.Invoke(this, new CanFrameReceivedEventArgs(frame, DateTime.UtcNow));
    }

    private void OnLineSourceFaulted(object? sender, Exception ex) => Faulted?.Invoke(this, ex);

    public void Dispose()
    {
        _lineSource.LineReceived -= OnLineReceived;
        _lineSource.Faulted -= OnLineSourceFaulted;
        _lineSource.Dispose();
    }
}
