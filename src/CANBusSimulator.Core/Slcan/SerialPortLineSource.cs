using System.IO.Ports;
using System.Text;

namespace CANBusSimulator.Core.Slcan;

/// <summary>Default <see cref="ISlcanLineSource"/> backed by a real <see cref="SerialPort"/>.</summary>
public sealed class SerialPortLineSource : ISlcanLineSource
{
    private readonly SerialPort _port;
    private readonly StringBuilder _buffer = new();
    private readonly object _bufferLock = new();

    public event EventHandler<string>? LineReceived;
    public event EventHandler<Exception>? Faulted;

    public SerialPortLineSource(string portName, int serialBaudRate)
    {
        _port = new SerialPort(portName, serialBaudRate)
        {
            NewLine = "\r",
            ReadTimeout = 500,
            WriteTimeout = 500,
        };
        _port.DataReceived += OnDataReceived;
    }

    public bool IsOpen => _port.IsOpen;

    public void Open() => _port.Open();

    public void Write(string text) => _port.Write(text);

    public void Close() => _port.Close();

    private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        try
        {
            string chunk = _port.ReadExisting();
            lock (_bufferLock)
            {
                _buffer.Append(chunk);
                string pending = _buffer.ToString();
                int start = 0, cr;
                while ((cr = pending.IndexOf('\r', start)) >= 0)
                {
                    if (cr > start)
                        LineReceived?.Invoke(this, pending[start..cr]);
                    start = cr + 1;
                }
                _buffer.Clear();
                _buffer.Append(pending[start..]);
            }
        }
        catch (Exception ex)
        {
            Faulted?.Invoke(this, ex);
        }
    }

    public void Dispose()
    {
        _port.DataReceived -= OnDataReceived;
        _port.Dispose();
    }
}
