using CANBusSimulator.Core.Domain;
using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Simulation;

/// <summary>Raised once per parameter, per tick, after a frame has been sent - carries what the UI log needs.</summary>
public sealed class SimulationTickEventArgs : EventArgs
{
    public SimulatedParameter Parameter { get; }
    public CanFrame Frame { get; }
    public double Value { get; }
    public DateTime Timestamp { get; }

    public SimulationTickEventArgs(SimulatedParameter parameter, CanFrame frame, double value, DateTime timestamp)
    {
        Parameter = parameter;
        Frame = frame;
        Value = value;
        Timestamp = timestamp;
    }
}
