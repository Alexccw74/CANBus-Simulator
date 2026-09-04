using System.Net;
using System.Net.Sockets;
using CANBusSimulator.Core.Domain;
using NModbus;
using NModbus.Data;

namespace CANBusSimulator.ModbusSlave;

/// <summary>
/// Owns an NModbus TCP slave (server) and an in-memory holding-register store laid out by
/// <see cref="RegisterMap"/>. <see cref="UpdateParameter"/> is called by the decoder pipeline every
/// time a new CAN frame is decoded; it works purely against the in-memory store, so it (and therefore
/// the register layout) is fully unit-testable without ever starting the TCP listener.
/// </summary>
public sealed class ModbusSlaveService : IAsyncDisposable
{
    private readonly ModbusSlaveOptions _options;
    private readonly SlaveDataStore _dataStore = new();
    private readonly IModbusFactory _factory = new ModbusFactory();
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _listenTask;
    private ushort _heartbeat;

    public ModbusSlaveService(ModbusSlaveOptions options)
    {
        _options = options;
    }

    /// <summary>Starts listening for Modbus TCP master connections. Idempotent register state persists across restarts.</summary>
    public void Start()
    {
        _listener = new TcpListener(IPAddress.Any, _options.Port);
        _listener.Start();

        var slaveNetwork = _factory.CreateSlaveNetwork(_listener);
        var slave = _factory.CreateSlave(_options.UnitId, _dataStore);
        slaveNetwork.AddSlave(slave);

        _cts = new CancellationTokenSource();
        _listenTask = slaveNetwork.ListenAsync(_cts.Token);

        WriteLinkStatus(healthy: false);
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();
        _listener?.Stop();
        if (_listenTask is not null)
        {
            try
            {
                await _listenTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // expected on shutdown
            }
            catch (Exception)
            {
                // NModbus's accept loop can surface a SocketException (rather than
                // OperationCanceledException) when the listener is stopped mid-accept - also expected
                // once we've already requested shutdown above.
            }
        }
        _cts?.Dispose();
    }

    /// <summary>
    /// Writes one decoded parameter's status and value into its registers, bumps the heartbeat, and
    /// marks the link healthy. <paramref name="value"/> is <see langword="null"/> when the source
    /// reported the SAE "not available" sentinel for this reading.
    /// </summary>
    public void UpdateParameter(SimulatedParameter parameter, double? value)
    {
        int index = RegisterMap.IndexOf(parameter);
        ushort statusAddress = RegisterMap.ParameterStatusAddress(_options.RegisterBase, index);
        ushort valueAddress = RegisterMap.ParameterValueAddress(_options.RegisterBase, index);

        ushort status = value is null ? RegisterMap.ParameterStatusNotAvailable : RegisterMap.ParameterStatusValid;
        _dataStore.HoldingRegisters.WritePoints(statusAddress, new[] { status });

        (ushort first, ushort second) = ModbusFloatCodec.Encode((float)(value ?? 0), _options.FloatWordOrder);
        _dataStore.HoldingRegisters.WritePoints(valueAddress, new[] { first, second });

        unchecked { _heartbeat++; }
        _dataStore.HoldingRegisters.WritePoints(RegisterMap.HeartbeatAddress(_options.RegisterBase), new[] { _heartbeat });
        WriteLinkStatus(healthy: true);
    }

    /// <summary>Reads a single holding register - used for diagnostics and tests.</summary>
    public ushort ReadRegister(ushort address) => _dataStore.HoldingRegisters.ReadPoints(address, 1)[0];

    private void WriteLinkStatus(bool healthy)
    {
        ushort value = (ushort)(healthy ? 1 : 0);
        _dataStore.HoldingRegisters.WritePoints(RegisterMap.LinkStatusAddress(_options.RegisterBase), new[] { value });
    }
}
