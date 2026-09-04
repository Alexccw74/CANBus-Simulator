namespace CANBusSimulator.Core.Domain;

/// <summary>
/// The curated set of SAE J1939 engine parameters this simulator can emit. Adding a new parameter
/// is a one-line addition here; no other code needs to change (see <see cref="J1939.J1939FrameBuilder"/>).
/// </summary>
public static class ParameterCatalog
{
    public const string EngineSpeedKey = "EngineSpeed";
    public const string CoolantTempKey = "CoolantTemp";
    public const string OilPressureKey = "OilPressure";
    public const string FuelLevelKey = "FuelLevel";
    public const string BatteryVoltageKey = "BatteryVoltage";
    public const string BoostPressureKey = "BoostPressure";

    public static IReadOnlyList<SimulatedParameter> All { get; } = new List<SimulatedParameter>
    {
        // PGN 61444 (EEC1) - Electronic Engine Controller 1
        new(EngineSpeedKey, "Engine Speed", "rpm",
            Pgn: 61444, Spn: 190, DefaultPriority: 3,
            StartByte: 3, LengthBytes: 2, Resolution: 0.125, Offset: 0,
            MinPhysical: 0, MaxPhysical: 8031.875),

        // PGN 65262 (ET1) - Engine Temperature 1
        new(CoolantTempKey, "Coolant Temperature", "°C",
            Pgn: 65262, Spn: 110, DefaultPriority: 6,
            StartByte: 0, LengthBytes: 1, Resolution: 1, Offset: -40,
            MinPhysical: -40, MaxPhysical: 210),

        // PGN 65263 (EFL/P1) - Engine Fluid Level/Pressure 1
        new(OilPressureKey, "Oil Pressure", "kPa",
            Pgn: 65263, Spn: 100, DefaultPriority: 6,
            StartByte: 3, LengthBytes: 1, Resolution: 4, Offset: 0,
            MinPhysical: 0, MaxPhysical: 1000),

        // PGN 65276 (DFLP) - Dash Display / Fuel Level
        new(FuelLevelKey, "Fuel Level", "%",
            Pgn: 65276, Spn: 96, DefaultPriority: 6,
            StartByte: 1, LengthBytes: 1, Resolution: 0.4, Offset: 0,
            MinPhysical: 0, MaxPhysical: 100),

        // PGN 65271 (VEP1) - Vehicle Electrical Power 1
        new(BatteryVoltageKey, "Battery Voltage", "V",
            Pgn: 65271, Spn: 168, DefaultPriority: 6,
            StartByte: 4, LengthBytes: 2, Resolution: 0.05, Offset: 0,
            MinPhysical: 0, MaxPhysical: 32),

        // PGN 65270 (IC1) - Inlet/Exhaust Conditions 1
        new(BoostPressureKey, "Boost Pressure", "kPa",
            Pgn: 65270, Spn: 102, DefaultPriority: 6,
            StartByte: 1, LengthBytes: 1, Resolution: 2, Offset: 0,
            MinPhysical: 0, MaxPhysical: 500),
    };

    public static SimulatedParameter ByKey(string key) =>
        All.First(p => p.Key == key);
}
