using CANBusSimulator.Service;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<CanReceiverOptions>(builder.Configuration.GetSection("CanReceiver"));
builder.Services.Configure<ModbusServiceOptions>(builder.Configuration.GetSection("Modbus"));
builder.Services.AddHostedService<Worker>();

// No-op on non-Windows platforms/when not running under the Service Control Manager, so this also
// runs fine as a plain console app (e.g. `dotnet run`) during development or on Linux.
builder.Services.AddWindowsService(options => options.ServiceName = "CANBusSimulator.Service");
if (OperatingSystem.IsWindows())
    builder.Logging.AddEventLog();

var host = builder.Build();
host.Run();
