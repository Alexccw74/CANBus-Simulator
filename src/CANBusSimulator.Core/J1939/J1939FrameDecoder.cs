using CANBusSimulator.Core.Domain;

namespace CANBusSimulator.Core.J1939;

/// <summary>
/// Decodes a received CAN frame back into physical parameter values. The exact inverse of
/// <see cref="J1939FrameBuilder"/>: generic over <see cref="Domain.ParameterCatalog"/>, so a frame
/// carrying a PGN matched by more than one catalog entry decodes every matching SPN from the same
/// 8-byte payload (not needed today - every current parameter has a distinct PGN - but supported).
/// </summary>
public sealed class J1939FrameDecoder
{
    public IReadOnlyList<DecodedValue> Decode(CanFrame frame)
    {
        J1939Id.Decode(frame.Id, out _, out uint pgn, out byte sourceAddress);
        var timestamp = DateTime.UtcNow;

        var results = new List<DecodedValue>();
        foreach (var definition in ParameterCatalog.All)
        {
            if (definition.Pgn != pgn)
                continue;
            if (definition.StartByte + definition.LengthBytes > frame.Dlc)
                continue; // frame too short to carry this SPN

            long raw = definition.LengthBytes == 1
                ? frame.Data[definition.StartByte]
                : frame.Data[definition.StartByte] | ((long)frame.Data[definition.StartByte + 1] << 8);

            bool notAvailable = definition.LengthBytes == 1 ? raw >= 0xFE : raw >= 0xFEFF;
            double? value = notAvailable ? null : raw * definition.Resolution + definition.Offset;

            results.Add(new DecodedValue(definition, value, sourceAddress, timestamp));
        }
        return results;
    }
}
