using CANBusSimulator.Core.Domain;

namespace CANBusSimulator.Core.Simulation;

/// <summary>
/// Mutable per-run state layered on top of a static <see cref="SimulatedParameter"/>: whether it is
/// enabled, its user-configured range, its live current value, and its value generator. One instance
/// per row in the UI's parameter grid.
/// </summary>
public sealed class ParameterRuntimeState
{
    public SimulatedParameter Definition { get; }
    public bool Enabled { get; set; } = true;
    public double UserMin { get; set; }
    public double UserMax { get; set; }
    public double CurrentValue { get; private set; }

    private readonly ValueGenerator _generator;

    public ParameterRuntimeState(SimulatedParameter definition, double userMin, double userMax, Random? rng = null)
    {
        Definition = definition;
        UserMin = userMin;
        UserMax = userMax;
        CurrentValue = (userMin + userMax) / 2;
        _generator = new ValueGenerator(CurrentValue, rng);
    }

    /// <summary>Advances this parameter's simulated value and returns it.</summary>
    public double Advance()
    {
        CurrentValue = _generator.Next(UserMin, UserMax);
        return CurrentValue;
    }
}
