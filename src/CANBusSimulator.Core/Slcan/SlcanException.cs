namespace CANBusSimulator.Core.Slcan;

/// <summary>Typed error for SLCAN/serial port failures, with a message suitable for display to the user.</summary>
public sealed class SlcanException : Exception
{
    public SlcanException(string message) : base(message) { }
    public SlcanException(string message, Exception innerException) : base(message, innerException) { }
}
