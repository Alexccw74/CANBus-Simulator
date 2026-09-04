using CANBusSimulator.Core.Domain;

namespace CANBusSimulator.Core.J1939;

/// <summary>
/// Encodes a single simulated parameter's physical value into a full 8-byte J1939 CAN frame.
/// Generic over <see cref="SimulatedParameter"/> - adding a new parameter to the catalog requires
/// no change here. Bytes not occupied by the parameter are filled with 0xFF ("not available"),
/// matching the SAE convention so real J1939 tooling doesn't misread them as valid zero readings.
/// </summary>
public sealed class J1939FrameBuilder
{
    private const int DataLength = 8;

    public CanFrame BuildFrame(SimulatedParameter definition, double physicalValue, byte sourceAddress)
    {
        var data = new byte[DataLength];
        Array.Fill(data, (byte)0xFF);

        long maxRaw = definition.LengthBytes == 2 ? 0xFFFEL : 0xFEL; // reserve 0xFF.. as "not available"/error
        long raw = (long)Math.Round((physicalValue - definition.Offset) / definition.Resolution);
        raw = Math.Clamp(raw, 0, maxRaw);

        if (definition.LengthBytes == 1)
        {
            data[definition.StartByte] = (byte)raw;
        }
        else
        {
            data[definition.StartByte] = (byte)(raw & 0xFF);
            data[definition.StartByte + 1] = (byte)((raw >> 8) & 0xFF);
        }

        uint id = J1939Id.Build(definition.DefaultPriority, definition.Pgn, sourceAddress);
        return new CanFrame(id, Extended: true, Dlc: DataLength, Data: data);
    }
}
