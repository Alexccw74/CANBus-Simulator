namespace CANBusSimulator.Core.Domain;

/// <summary>
/// Static metadata for one simulated J1939 engine parameter: which PGN/SPN it belongs to, where it
/// lives in the 8-byte data payload, and how to convert a physical value to/from the raw wire value.
/// </summary>
/// <param name="Key">Stable identifier used to look up per-model default ranges, e.g. "EngineSpeed".</param>
/// <param name="DisplayName">Human-readable name shown in the UI.</param>
/// <param name="Unit">Physical unit shown in the UI, e.g. "rpm".</param>
/// <param name="Pgn">SAE J1939 Parameter Group Number.</param>
/// <param name="Spn">SAE J1939 Suspect Parameter Number, for reference/display only.</param>
/// <param name="DefaultPriority">CAN ID priority field (0 = highest, 7 = lowest) conventionally used for this PGN.</param>
/// <param name="StartByte">0-based offset into the 8-byte data field (SAE spec byte numbers are 1-based).</param>
/// <param name="LengthBytes">1 or 2 bytes, little-endian when 2.</param>
/// <param name="Resolution">Physical units per raw bit, e.g. 0.125 rpm/bit.</param>
/// <param name="Offset">Physical value at raw = 0, e.g. -40 for a temperature SPN.</param>
/// <param name="MinPhysical">Lower bound the UI/encoder will accept.</param>
/// <param name="MaxPhysical">Upper bound the UI/encoder will accept.</param>
public sealed record SimulatedParameter(
    string Key,
    string DisplayName,
    string Unit,
    uint Pgn,
    ushort Spn,
    byte DefaultPriority,
    int StartByte,
    int LengthBytes,
    double Resolution,
    double Offset,
    double MinPhysical,
    double MaxPhysical);
