namespace CANBusSimulator.Core.Domain;

/// <summary>
/// A specific engine model belonging to a <see cref="Brand"/>. <see cref="DefaultRanges"/> only supplies
/// convenience defaults shown in the UI when the model is selected (keyed by <see cref="SimulatedParameter.Key"/>);
/// it never affects J1939 frame encoding.
/// </summary>
public sealed record EngineModel(
    string Id,
    string BrandId,
    string Name,
    IReadOnlyDictionary<string, ParameterRangeDefaults> DefaultRanges);
