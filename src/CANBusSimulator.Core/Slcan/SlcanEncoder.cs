using System.Text;
using CANBusSimulator.Core.J1939;

namespace CANBusSimulator.Core.Slcan;

/// <summary>Encodes <see cref="CanFrame"/> values into Lawicel/SLCAN ASCII wire commands.</summary>
public static class SlcanEncoder
{
    /// <summary>
    /// Encodes an extended (29-bit) data frame as "T" + 8 hex ID digits + 1 hex DLC digit + DLC*2 hex
    /// data digits + CR, e.g. "T18FEF12300FA1234FFFFFF\r".
    /// </summary>
    public static string EncodeExtendedFrame(CanFrame frame)
    {
        if (!frame.Extended)
            throw new ArgumentException("Only extended (29-bit) frames are supported.", nameof(frame));
        if (frame.Dlc > 8)
            throw new ArgumentOutOfRangeException(nameof(frame), "DLC must be 0-8.");

        var sb = new StringBuilder(1 + 8 + 1 + frame.Dlc * 2 + 1);
        sb.Append('T');
        sb.Append(frame.Id.ToString("X8"));
        sb.Append(frame.Dlc.ToString("X1"));
        for (int i = 0; i < frame.Dlc; i++)
            sb.Append(frame.Data[i].ToString("X2"));
        sb.Append('\r');
        return sb.ToString();
    }

    /// <summary>SLCAN command to set the CAN bitrate, e.g. "S6\r" for 500 kbit/s.</summary>
    public static string EncodeSetBitrate(char slcanBitrateCode) => $"S{slcanBitrateCode}\r";

    /// <summary>SLCAN command to open the channel.</summary>
    public const string OpenChannel = "O\r";

    /// <summary>SLCAN command to close the channel.</summary>
    public const string CloseChannel = "C\r";
}
