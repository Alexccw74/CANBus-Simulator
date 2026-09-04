namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Maps CAN bus bitrates (distinct from the serial port's own baud rate) to the SLCAN 'S' setup code.
/// </summary>
public static class BaudRateMap
{
    public static readonly IReadOnlyDictionary<int, char> CanBitrateToSlcanCode = new Dictionary<int, char>
    {
        [10_000] = '0',
        [20_000] = '1',
        [50_000] = '2',
        [100_000] = '3',
        [125_000] = '4',
        [250_000] = '5', // SAE J1939 standard bus speed
        [500_000] = '6',
        [800_000] = '7',
        [1_000_000] = '8',
    };

    public static char CodeFor(int canBitrateBps)
    {
        if (!CanBitrateToSlcanCode.TryGetValue(canBitrateBps, out var code))
            throw new ArgumentOutOfRangeException(nameof(canBitrateBps), canBitrateBps, "Unsupported CAN bitrate.");
        return code;
    }
}
