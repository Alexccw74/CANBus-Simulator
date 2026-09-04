namespace CANBusSimulator.Core.J1939;

/// <summary>
/// Builds 29-bit extended J1939 CAN identifiers. All parameters currently in <see cref="Domain.ParameterCatalog"/>
/// use PDU2 (broadcast) PGNs, i.e. PF &gt;= 240, so the PGN alone (which already encodes PF/PS/DP) can be placed
/// directly into the identifier with no destination-address substitution.
/// </summary>
public static class J1939Id
{
    /// <summary>
    /// Builds a 29-bit CAN identifier as (priority &lt;&lt; 26) | (pgn &lt;&lt; 8) | sourceAddress.
    /// </summary>
    /// <param name="priority">3-bit priority, 0 (highest) to 7 (lowest).</param>
    /// <param name="pgn">The Parameter Group Number (up to 18 bits, includes Data Page bit).</param>
    /// <param name="sourceAddress">The sending node's J1939 source address.</param>
    public static uint Build(byte priority, uint pgn, byte sourceAddress)
    {
        return ((uint)(priority & 0x7) << 26)
             | ((pgn & 0x3FFFF) << 8)
             | sourceAddress;
    }

    /// <summary>Inverse of <see cref="Build"/>: extracts priority, PGN, and source address from a 29-bit CAN ID.</summary>
    public static void Decode(uint canId, out byte priority, out uint pgn, out byte sourceAddress)
    {
        priority = (byte)((canId >> 26) & 0x7);
        pgn = (canId >> 8) & 0x3FFFF;
        sourceAddress = (byte)(canId & 0xFF);
    }
}
