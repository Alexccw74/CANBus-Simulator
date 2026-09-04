using System.Globalization;
using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Slcan;

/// <summary>
/// Parses incoming SLCAN/Lawicel ASCII lines into <see cref="CanFrame"/> values - the exact inverse
/// of <see cref="SlcanEncoder.EncodeExtendedFrame"/>. Lines that aren't a data frame (adapter
/// acknowledgements, echoed setup commands, malformed input) are simply not parsed.
/// </summary>
public static class SlcanFrameParser
{
    /// <summary>
    /// Attempts to parse one line (terminator already stripped) such as "T18FEF90087DFFFFFFFFFFFFFF"
    /// (extended, 29-bit) or "t12387DFFFFFFFFFFFFFF" (standard, 11-bit) into a <see cref="CanFrame"/>.
    /// </summary>
    public static bool TryParse(string line, out CanFrame frame)
    {
        frame = default;
        if (string.IsNullOrEmpty(line))
            return false;

        bool extended = line[0] switch
        {
            'T' => true,
            't' => false,
            _ => false,
        };
        if (line[0] != 'T' && line[0] != 't')
            return false;

        int idLength = extended ? 8 : 3;
        int dlcIndex = 1 + idLength;
        if (line.Length <= dlcIndex)
            return false;

        if (!uint.TryParse(line.AsSpan(1, idLength), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint id))
            return false;

        if (!byte.TryParse(line.AsSpan(dlcIndex, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte dlc)
            || dlc > 8)
            return false;

        int dataStart = dlcIndex + 1;
        if (line.Length < dataStart + dlc * 2)
            return false;

        var data = new byte[8];
        for (int i = 0; i < dlc; i++)
        {
            if (!byte.TryParse(line.AsSpan(dataStart + i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out data[i]))
                return false;
        }

        frame = new CanFrame(id, extended, dlc, data);
        return true;
    }
}
