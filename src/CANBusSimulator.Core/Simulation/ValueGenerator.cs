namespace CANBusSimulator.Core.Simulation;

/// <summary>
/// Produces a slowly-varying, bounded value that mean-reverts toward an occasionally-changing random
/// target within [min, max]. Looks like organic sensor drift rather than pure random noise or a rigid
/// sine wave, while always staying within the user-configured range.
/// </summary>
public sealed class ValueGenerator
{
    private readonly Random _rng;
    private double _value;
    private double _target;

    public ValueGenerator(double initialValue, Random? rng = null)
    {
        _rng = rng ?? Random.Shared;
        _value = initialValue;
        _target = initialValue;
    }

    /// <summary>Advances and returns the next value, clamped to [min, max].</summary>
    public double Next(double min, double max)
    {
        if (min > max)
            (min, max) = (max, min);

        double range = Math.Max(max - min, double.Epsilon);

        // Occasionally pick a new random target within range.
        if (_rng.NextDouble() < 0.02)
            _target = min + _rng.NextDouble() * range;

        double drift = (_target - _value) * 0.1;
        double noise = (_rng.NextDouble() - 0.5) * range * 0.01;
        _value = Math.Clamp(_value + drift + noise, min, max);
        return _value;
    }
}
