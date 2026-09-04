using CANBusSimulator.Core.Slcan;

namespace CANBusSimulator.Core.CanHardware;

/// <summary>
/// <see cref="ICanReceiver"/> for the PCM-9366's onboard SJA1000-based CAN interface, reached on
/// Windows via an Advantech CAN driver DLL.
///
/// NOT YET FUNCTIONAL. This is a skeleton: the exact P/Invoke signatures depend on which SDK
/// Advantech shipped for this specific card, and that SDK wasn't available while writing this class.
/// Every native call below is a placeholder that throws <see cref="NotSupportedException"/>. To
/// finish it:
///   1. Get the Advantech CAN card's SDK manual/header (commonly a DLL such as CanApi.dll/AdvCan.dll,
///      or a ZLG/VCI-style API - the exact name and export list vary by which driver Advantech
///      bundled for this card).
///   2. Add the real <c>[DllImport]</c> declarations for: open device, set bitrate, start/stop
///      channel, read one frame (or register a receive callback), close device.
///   3. Fill in <see cref="Open"/>, <see cref="PollLoop"/>, and <see cref="Close"/> below.
/// Nothing else in this codebase needs to change once this file is completed - the rest of the
/// pipeline (decoding, Modbus mapping, the Windows Service host) consumes any <see cref="ICanReceiver"/>
/// identically, whether it's this class or <see cref="SlcanReceiver"/>.
/// </summary>
public sealed class AdvantechCanReceiver : ICanReceiver
{
    private readonly int _deviceIndex;
    private readonly int _channelIndex;

    // Assigned once Open() actually starts the poll loop against the real SDK - see the TODOs below.
#pragma warning disable CS0649
    private CancellationTokenSource? _pollLoopCts;
    private Task? _pollLoopTask;
#pragma warning restore CS0649

    // Unused until PollLoop() is implemented against the real SDK - see the TODOs below.
#pragma warning disable CS0067
    public event EventHandler<CanFrameReceivedEventArgs>? FrameReceived;
    public event EventHandler<Exception>? Faulted;
#pragma warning restore CS0067

    public AdvantechCanReceiver(int deviceIndex = 0, int channelIndex = 0)
    {
        _deviceIndex = deviceIndex;
        _channelIndex = channelIndex;
    }

    public void Open(int canBitrateBps)
    {
        // TODO once the SDK is available, the intended shape is:
        //   _handle = NativeMethods.OpenDevice(_deviceIndex);
        //   NativeMethods.SetBitrate(_handle, _channelIndex, canBitrateBps); // or a vendor bitrate-table index
        //   NativeMethods.StartChannel(_handle, _channelIndex);
        //   _pollLoopCts = new CancellationTokenSource();
        //   _pollLoopTask = Task.Run(() => PollLoop(_pollLoopCts.Token));
        throw new NotSupportedException(
            "AdvantechCanReceiver is a skeleton: the Advantech CAN driver DLL's exact P/Invoke " +
            "signatures are not yet known. Supply the SDK's function names/signatures (device open, " +
            "set bitrate, read frame, close) to complete this class - see the TODOs in this file.");
    }

    private void PollLoop(CancellationToken token)
    {
        // TODO once the SDK is available, replace with a blocking/polling call into the native
        // driver that returns the next received frame, e.g.:
        //   while (!token.IsCancellationRequested)
        //   {
        //       if (NativeMethods.ReadFrame(_handle, _channelIndex, out var native, timeoutMs: 100))
        //       {
        //           var frame = new J1939.CanFrame(native.Id, native.Extended, native.Dlc, native.Data);
        //           FrameReceived?.Invoke(this, new CanFrameReceivedEventArgs(frame, DateTime.UtcNow));
        //       }
        //   }
        // Any exception from the native layer should be caught here and raised via Faulted, not thrown
        // across the background task boundary.
        throw new NotSupportedException("AdvantechCanReceiver.PollLoop is not implemented - see Open().");
    }

    public void Close()
    {
        _pollLoopCts?.Cancel();
        try
        {
            _pollLoopTask?.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // Best-effort: we're tearing down regardless.
        }
        // TODO once the SDK is available: NativeMethods.StopChannel(_handle, _channelIndex); NativeMethods.CloseDevice(_handle);
    }

    public void Dispose() => Close();
}
