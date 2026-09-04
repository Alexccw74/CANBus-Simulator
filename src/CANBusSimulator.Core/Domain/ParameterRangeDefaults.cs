namespace CANBusSimulator.Core.Domain;

/// <summary>Suggested initial min/max range for one simulated parameter on a given engine model.</summary>
public sealed record ParameterRangeDefaults(double DefaultMin, double DefaultMax);
