using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;

namespace CANBusSimulator.Core.Simulation;

/// <summary>
/// Drives the simulation: on each tick, for every enabled parameter it advances the value, builds the
/// J1939 frame, transmits it via <see cref="SlcanPort"/>, and raises <see cref="FrameSent"/> so the UI
/// can log it. Uses <see cref="System.Threading.Timer"/> (not a WinForms UI-thread timer) so simulation
/// timing isn't affected by UI load; consumers must marshal event handling back to the UI thread.
/// </summary>
public sealed class SimulationEngine : IDisposable
{
    private readonly SlcanPort _port;
    private readonly IReadOnlyList<ParameterRuntimeState> _parameters;
    private readonly byte _sourceAddress;
    private readonly J1939FrameBuilder _builder = new();
    private readonly Timer _timer;
    private int _ticking; // 0 = idle, 1 = a tick is in progress (guards against overlapping ticks)

    public event EventHandler<SimulationTickEventArgs>? FrameSent;
    public event EventHandler<Exception>? Faulted;

    public bool IsRunning { get; private set; }

    public SimulationEngine(SlcanPort port, IReadOnlyList<ParameterRuntimeState> parameters, byte sourceAddress)
    {
        _port = port;
        _parameters = parameters;
        _sourceAddress = sourceAddress;
        _timer = new Timer(OnTick, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>Starts periodic simulation at the given global rate.</summary>
    public void Start(int intervalMs)
    {
        IsRunning = true;
        _timer.Change(0, intervalMs);
    }

    public void Stop()
    {
        IsRunning = false;
        _timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void OnTick(object? state)
    {
        if (Interlocked.Exchange(ref _ticking, 1) == 1)
            return; // previous tick still in flight (e.g. slow/stalled port) - skip this one rather than overlap

        try
        {
            foreach (var p in _parameters)
            {
                if (!p.Enabled)
                    continue;

                double value = p.Advance();
                CanFrame frame = _builder.BuildFrame(p.Definition, value, _sourceAddress);
                _port.SendFrame(frame);
                FrameSent?.Invoke(this, new SimulationTickEventArgs(p.Definition, frame, value, DateTime.Now));
            }
        }
        catch (Exception ex)
        {
            Stop();
            Faulted?.Invoke(this, ex);
        }
        finally
        {
            Interlocked.Exchange(ref _ticking, 0);
        }
    }

    public void Dispose() => _timer.Dispose();
}
