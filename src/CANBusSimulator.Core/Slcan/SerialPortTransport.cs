using System.IO.Ports;

namespace CANBusSimulator.Core.Slcan;

/// <summary>Default <see cref="ISlcanTransport"/> backed by a real <see cref="SerialPort"/>.</summary>
public sealed class SerialPortTransport : ISlcanTransport
{
    private readonly SerialPort _port;

    public SerialPortTransport(string portName, int serialBaudRate)
    {
        _port = new SerialPort(portName, serialBaudRate)
        {
            NewLine = "\r",
            ReadTimeout = 500,
            WriteTimeout = 500,
        };
    }

    public bool IsOpen => _port.IsOpen;

    public void Open() => _port.Open();

    public void Write(string text) => _port.Write(text);

    public void Close() => _port.Close();

    public void Dispose() => _port.Dispose();
}
