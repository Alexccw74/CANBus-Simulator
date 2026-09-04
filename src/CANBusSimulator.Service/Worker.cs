using CANBusSimulator.Core.CanHardware;
using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;
using CANBusSimulator.ModbusSlave;
using Microsoft.Extensions.Options;

namespace CANBusSimulator.Service;

/// <summary>
/// Opens the configured <see cref="ICanReceiver"/>, decodes every received frame with
/// <see cref="J1939FrameDecoder"/>, and forwards the results into <see cref="ModbusSlaveService"/>.
/// If the receiver fails to open or faults mid-run, this retries on a fixed delay rather than
/// crashing the whole service - this keeps a real hardware/COM-port hiccup from taking the Modbus
/// slave itself offline.
/// </summary>
public sealed class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly CanReceiverOptions _canOptions;
    private readonly ModbusSlaveOptions _modbusOptions;
    private readonly J1939FrameDecoder _decoder = new();

    private ModbusSlaveService? _modbusService;
    private ICanReceiver? _receiver;
    private TaskCompletionSource<bool>? _faultSignal;

    public Worker(
        ILogger<Worker> logger,
        IOptions<CanReceiverOptions> canOptions,
        IOptions<ModbusServiceOptions> modbusOptions)
    {
        _logger = logger;
        _canOptions = canOptions.Value;

        var modbus = modbusOptions.Value;
        _modbusOptions = new ModbusSlaveOptions
        {
            Port = modbus.Port,
            UnitId = modbus.UnitId,
            RegisterBase = modbus.RegisterBase,
            FloatWordOrder = Enum.Parse<ModbusFloatWordOrder>(modbus.FloatWordOrder, ignoreCase: true),
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _modbusService = new ModbusSlaveService(_modbusOptions);
        _modbusService.Start();
        _logger.LogInformation(
            "Modbus TCP slave listening on port {Port} (unit id {UnitId}, register base {RegisterBase}).",
            _modbusOptions.Port, _modbusOptions.UnitId, _modbusOptions.RegisterBase);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                OpenReceiver();
                await WaitUntilFaultedOrCancelledAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CAN receiver ({Type}) failed to open.", _canOptions.Type);
            }
            finally
            {
                CloseReceiver();
            }

            if (stoppingToken.IsCancellationRequested)
                break;

            _logger.LogInformation("Retrying the CAN receiver in {Delay}s.", _canOptions.ReconnectDelaySeconds);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_canOptions.ReconnectDelaySeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        CloseReceiver();
        if (_modbusService is not null)
            await _modbusService.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }

    private void OpenReceiver()
    {
        _receiver = _canOptions.Type.Equals("Advantech", StringComparison.OrdinalIgnoreCase)
            ? new AdvantechCanReceiver(_canOptions.AdvantechDeviceIndex, _canOptions.AdvantechChannelIndex)
            : new SlcanReceiver(_canOptions.ComPort, _canOptions.SerialBaudRate);

        _receiver.FrameReceived += OnFrameReceived;
        _receiver.Faulted += OnReceiverFaulted;
        _receiver.Open(_canOptions.CanBitrateBps);
        _logger.LogInformation("CAN receiver ({Type}) opened.", _canOptions.Type);
    }

    private void CloseReceiver()
    {
        if (_receiver is null)
            return;

        _receiver.FrameReceived -= OnFrameReceived;
        _receiver.Faulted -= OnReceiverFaulted;
        try
        {
            _receiver.Close();
        }
        catch
        {
            // Best-effort: we're tearing down regardless.
        }
        _receiver.Dispose();
        _receiver = null;
    }

    private async Task WaitUntilFaultedOrCancelledAsync(CancellationToken stoppingToken)
    {
        _faultSignal = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        using var registration = stoppingToken.Register(() => _faultSignal.TrySetCanceled(stoppingToken));
        await _faultSignal.Task;
    }

    private void OnFrameReceived(object? sender, CanFrameReceivedEventArgs e)
    {
        foreach (var decoded in _decoder.Decode(e.Frame))
            _modbusService?.UpdateParameter(decoded.Parameter, decoded.Value);
    }

    private void OnReceiverFaulted(object? sender, Exception ex)
    {
        _logger.LogWarning(ex, "CAN receiver faulted.");
        _faultSignal?.TrySetResult(true);
    }
}
