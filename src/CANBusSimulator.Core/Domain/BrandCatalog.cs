namespace CANBusSimulator.Core.Domain;

/// <summary>
/// Curated, hard-coded catalog of engine brands and models used to populate the UI's cascading
/// Brand/Model combo boxes and to seed sensible default parameter ranges. Purely presentational data;
/// it never affects J1939 frame encoding. Adding a brand or model is a one-line addition here.
/// </summary>
public static class BrandCatalog
{
    public static IReadOnlyList<Brand> Brands { get; } = new List<Brand>
    {
        new("CAT", "Caterpillar"),
        new("YANMAR", "YANMAR"),
        new("CUMMINS", "Cummins"),
        new("JOHNDEERE", "John Deere"),
        new("VOLVOPENTA", "Volvo Penta"),
        new("PERKINS", "Perkins"),
    };

    public static IReadOnlyList<EngineModel> Models { get; } = new List<EngineModel>
    {
        // Caterpillar - industrial / genset diesels
        Model("CAT", "C9", rpmMax: 2200, nominalVoltage: 24, boostMax: 220),
        Model("CAT", "C15", rpmMax: 2100, nominalVoltage: 24, boostMax: 260),
        Model("CAT", "C18", rpmMax: 2100, nominalVoltage: 24, boostMax: 280),
        Model("CAT", "3406E", rpmMax: 2100, nominalVoltage: 24, boostMax: 240),
        Model("CAT", "3512", rpmMax: 1800, nominalVoltage: 24, boostMax: 300),

        // YANMAR - marine diesels
        Model("YANMAR", "4JH", rpmMax: 3800, nominalVoltage: 12, boostMax: 120),
        Model("YANMAR", "6LY", rpmMax: 3300, nominalVoltage: 12, boostMax: 180),
        Model("YANMAR", "4LV", rpmMax: 3800, nominalVoltage: 12, boostMax: 150),

        // Cummins
        Model("CUMMINS", "QSB", rpmMax: 2500, nominalVoltage: 24, boostMax: 200),
        Model("CUMMINS", "QSX", rpmMax: 2100, nominalVoltage: 24, boostMax: 280),

        // John Deere
        Model("JOHNDEERE", "PowerTech", rpmMax: 2400, nominalVoltage: 12, boostMax: 200),

        // Volvo Penta - marine/industrial
        Model("VOLVOPENTA", "D4", rpmMax: 3500, nominalVoltage: 12, boostMax: 150),
        Model("VOLVOPENTA", "D6", rpmMax: 3500, nominalVoltage: 12, boostMax: 170),
        Model("VOLVOPENTA", "D11", rpmMax: 2300, nominalVoltage: 24, boostMax: 220),
        Model("VOLVOPENTA", "D13", rpmMax: 2200, nominalVoltage: 24, boostMax: 240),

        // Perkins - industrial
        Model("PERKINS", "1100 Series", rpmMax: 2800, nominalVoltage: 12, boostMax: 160),
        Model("PERKINS", "1300 Series", rpmMax: 2500, nominalVoltage: 24, boostMax: 200),
    };

    public static IEnumerable<EngineModel> ModelsForBrand(string brandId) =>
        Models.Where(m => m.BrandId == brandId);

    private static EngineModel Model(string brandId, string name, double rpmMax, double nominalVoltage, double boostMax)
    {
        var ranges = new Dictionary<string, ParameterRangeDefaults>
        {
            [ParameterCatalog.EngineSpeedKey] = new(rpmMax * 0.30, rpmMax * 0.85),
            [ParameterCatalog.CoolantTempKey] = new(70, 95),
            [ParameterCatalog.OilPressureKey] = new(150, 450),
            [ParameterCatalog.FuelLevelKey] = new(20, 90),
            [ParameterCatalog.BatteryVoltageKey] = new(nominalVoltage * 0.92, nominalVoltage * 1.15),
            [ParameterCatalog.BoostPressureKey] = new(boostMax * 0.20, boostMax),
        };
        return new EngineModel($"{brandId}-{name}", brandId, name, ranges);
    }
}
