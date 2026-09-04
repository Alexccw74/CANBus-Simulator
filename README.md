# CAN Bus Simulator

Two complementary tools for working with a diesel engine's SAE J1939 CAN bus traffic without a real
engine attached:

- **`CANBusSimulator.App`** - a Windows Forms desktop app that *simulates* engine telemetry and
  transmits it over a COM port.
- **`CANBusSimulator.Service`** - a Windows Service that does the reverse: *receives* real (or
  simulated) J1939 CAN traffic from a COM port or a native CAN adapter, decodes it, and republishes
  it as Modbus TCP holding registers for a SCADA/HMI/PLC to poll.

Both share the same `CANBusSimulator.Core` library, so the exact PGN/SPN metadata the simulator
encodes with is also what the service decodes with.

## The simulator (`CANBusSimulator.App`)

You pick an engine **brand and model** (Caterpillar, YANMAR, Cummins, John Deere, Volvo Penta,
Perkins), pick an available **COM port**, set a **min/max range** for each simulated engine
parameter, and start the simulation. The app then streams realistic, slowly-varying values out the
serial port using the **SLCAN (Lawicel) ASCII protocol** - the same wire protocol used by common
USB-CAN adapters (CANable, USBtin, etc.) - encoded as genuine **SAE J1939** frames (Engine Speed,
Coolant Temperature, Oil Pressure, Fuel Level, Battery Voltage, Boost Pressure).

## The decoder/Modbus service (`CANBusSimulator.Service`)

Opens a CAN receiver (either an SLCAN-speaking COM port, or - once wired up - a native Advantech CAN
card such as the PCM-9366's onboard SJA1000-based interface), decodes every J1939 frame back into
physical engine values using the same catalog the simulator encodes with, and serves them over
Modbus TCP as a slave (server) that any Modbus master can poll. Runs as a native Windows Service via
the modern `Microsoft.Extensions.Hosting` generic host (`UseWindowsService`/`AddWindowsService`), but
also runs as a plain console app for local development - including on Linux, since only the actual
Windows Service *registration* is platform-specific.

See **"Decoder + Modbus Service" below** for configuration, the register map, and how to test the
whole pipeline without any real hardware.

## Project layout

```
CANBusSimulator.sln
src/
  CANBusSimulator.Core/          Portable class library (net8.0, no WinForms dependency):
                                  J1939 frame encoding AND decoding, SLCAN transmit + receive,
                                  brand/model catalog, simulation engine. Builds and is
                                  unit-tested on any OS.
  CANBusSimulator.App/           Windows Forms UI (net8.0-windows). References Core. Windows-only.
  CANBusSimulator.ModbusSlave/   Modbus TCP slave + holding-register map (net8.0, uses NModbus).
                                  References Core. Builds/tests on any OS.
  CANBusSimulator.Service/       Windows Service host (net8.0, Generic Host). References Core +
                                  ModbusSlave. Builds and runs as a console app on any OS; only
                                  the Windows Service registration itself is Windows-only.
tests/
  CANBusSimulator.Core.Tests/          xUnit tests for Core (frame encoding/decoding, SLCAN
                                        transmit/receive wire format, value generator bounds,
                                        SLCAN port/receiver open-close command sequences).
  CANBusSimulator.ModbusSlave.Tests/   xUnit tests for the float codec and register map.
```

The hard, correctness-critical logic (J1939 byte-packing/unpacking, SLCAN ASCII encoding/parsing,
bounded random-walk value generation, the Modbus register map) lives in plain `net8.0` libraries with
no WinForms dependency, fully covered by unit tests that run on Linux, macOS, or Windows. Only the
`CANBusSimulator.App` WinForms UI project requires Windows to build; `CANBusSimulator.Service` builds
and runs everywhere, with just its Windows Service registration being Windows-only.

## Building & running

**Core library + tests (any OS):**

```
dotnet build src/CANBusSimulator.Core/CANBusSimulator.Core.csproj
dotnet test tests/CANBusSimulator.Core.Tests/CANBusSimulator.Core.Tests.csproj
```

**Full app (Windows only):**

```
dotnet build CANBusSimulator.sln
dotnet run --project src/CANBusSimulator.App/CANBusSimulator.App.csproj
```

`CANBusSimulator.App` targets `net8.0-windows` with `UseWindowsForms=true` and cannot be built on
Linux/macOS (the .NET Windows Desktop SDK is Windows-only). It builds and runs on any Windows
machine with the .NET 8 SDK installed.

**Decoder/Modbus service + its tests (any OS):**

```
dotnet build src/CANBusSimulator.ModbusSlave/CANBusSimulator.ModbusSlave.csproj
dotnet test tests/CANBusSimulator.ModbusSlave.Tests/CANBusSimulator.ModbusSlave.Tests.csproj
dotnet run --project src/CANBusSimulator.Service    # runs as a console app; Ctrl+C to stop
```

## Using the simulator

1. Choose a **Brand** and **Model**. This fills in sensible default min/max ranges for every
   parameter (e.g. a marine high-speed diesel gets a higher default RPM range than an industrial
   genset engine) - it's a UI convenience only and never changes how frames are encoded.
2. Choose the **COM Port** (use *Refresh* to re-scan), the **Serial Baud** rate of your USB-CAN
   adapter's virtual COM port (commonly 115200), and the **CAN Bitrate** of the simulated bus
   (250 kbit/s is the SAE J1939 standard).
3. Adjust the **Min/Max** for any parameter in the grid, and untick **On** to exclude a parameter.
4. Set the **Update Rate (Hz)** and click **Start**. The log at the bottom shows every frame sent:
   timestamp, parameter, PGN, CAN ID, DLC, raw data bytes, and decoded value.
5. Click **Stop** (or close the window) to end the simulation and release the COM port.

## SAE J1939 background

All six simulated parameters are real SAE J1939 Suspect Parameter Numbers (SPNs), each carried in
its own Parameter Group Number (PGN) broadcast frame with a 29-bit extended CAN identifier:

| Parameter | PGN | SPN | Resolution | Offset |
|---|---|---|---|---|
| Engine Speed | 61444 (EEC1) | 190 | 0.125 rpm/bit | 0 |
| Coolant Temperature | 65262 (ET1) | 110 | 1 °C/bit | -40 |
| Oil Pressure | 65263 (EFL/P1) | 100 | 4 kPa/bit | 0 |
| Fuel Level | 65276 (DFLP) | 96 | 0.4 %/bit | 0 |
| Battery Voltage | 65271 (VEP1) | 168 | 0.05 V/bit | 0 |
| Boost Pressure | 65270 (IC1) | 102 | 2 kPa/bit | 0 |

Bytes not occupied by the simulated SPN in each 8-byte payload are filled with `0xFF`
("not available"), matching the SAE convention, so real J1939 tooling won't misread them as valid
zero readings. See `src/CANBusSimulator.Core/Domain/ParameterCatalog.cs` for the exact byte offsets.

## Decoder + Modbus Service

### Configuration (`src/CANBusSimulator.Service/appsettings.json`)

```json
{
  "CanReceiver": {
    "Type": "Slcan",              // "Slcan" (works today) or "Advantech" (needs the real SDK - see below)
    "ComPort": "COM3",
    "SerialBaudRate": 115200,
    "CanBitrateBps": 250000,
    "AdvantechDeviceIndex": 0,
    "AdvantechChannelIndex": 0,
    "ReconnectDelaySeconds": 5
  },
  "Modbus": {
    "Port": 5020,                 // 502 is the Modbus standard but needs admin/elevated rights on Windows
    "UnitId": 1,
    "RegisterBase": 0,
    "FloatWordOrder": "HighWordFirst"  // or "LowWordFirst" - different Modbus masters disagree on this
  }
}
```

Any setting can also be overridden with an environment variable, e.g. `CanReceiver__ComPort=COM5`
(the standard ASP.NET Core configuration convention) - handy for `sc create`'s service environment
or for testing without editing the file.

### Register map

Each decoded parameter gets a 3-register block; two global registers come first. `n` is the
parameter's index in `ParameterCatalog.All`'s declared order (Engine Speed=0, Coolant Temp=1, Oil
Pressure=2, Fuel Level=3, Battery Voltage=4, Boost Pressure=5). All addresses are relative to
`Modbus:RegisterBase` (default 0):

| Register | Contents |
|---|---|
| base + 0 | Heartbeat - rolling counter, increments every decoded frame (proves the service is alive) |
| base + 1 | Link status - 0 = no frame received yet, 1 = at least one frame decoded |
| base + 2 + 3n | Parameter *n* status - 0 = stale, 1 = valid, 2 = SAE "not available" |
| base + 3 + 3n | Parameter *n* value, register 1 of 2 (32-bit float, word order per config) |
| base + 4 + 3n | Parameter *n* value, register 2 of 2 |

Example: with the default base of 0, Engine Speed (index 0) is registers 2-4 (status, value-hi,
value-lo) and Coolant Temperature (index 1) is registers 5-7.

### Installing as a Windows Service

```
dotnet publish src/CANBusSimulator.Service -c Release -o C:\CANBusService
sc create CANBusSimulator.Service binPath= "C:\CANBusService\CANBusSimulator.Service.exe"
sc start CANBusSimulator.Service
```

Logs go to the Windows Event Log (source `CANBusSimulator.Service`) when actually running as a
service, and to the console when run directly with `dotnet run`/the published .exe from a terminal.

### The Advantech CAN SDK is not yet wired up - read this before setting `Type: "Advantech"`

Nobody involved in building this had the PCM-9366's Advantech CAN driver SDK in hand, so
`src/CANBusSimulator.Core/CanHardware/AdvantechCanReceiver.cs` is a **skeleton that throws
`NotSupportedException`** everywhere a real native call is needed - it's structured with the shape
common to SJA1000-based industrial CAN SDKs (open device → set bitrate → read loop/callback → close),
with `TODO` comments marking exactly where to add the real `[DllImport]` declarations once you have
the SDK's manual/header (the exact DLL name and function signatures vary by which driver Advantech
bundled for this card - commonly something like `CanApi.dll`/`AdvCan.dll`, or a ZLG/VCI-style API).
Nothing else in the codebase needs to change once that one file is completed - `Worker.cs` and the
Modbus mapping consume any `ICanReceiver` identically.

Until then, **`Type: "Slcan"` lets you validate everything else** - decoding, the register map, and
the Windows Service host - using the existing simulator as the CAN source:

### End-to-end test without any real hardware

1. On Windows: install a virtual COM port pair, e.g. [com0com](https://com0com.sourceforge.net/),
   giving you two linked ports (say `COM10`↔`COM11`). *(On Linux, `socat -d -d pty,raw,echo=0,link=/tmp/ttyA pty,raw,echo=0,link=/tmp/ttyB` does the same thing - this is exactly how the pipeline
   below was validated while building this service, feeding it hand-crafted SLCAN frames and
   confirming the right Modbus registers came back over a real NModbus TCP client.)*
2. Run `CANBusSimulator.App` pointed at one end of the pair (e.g. `COM10`), pick a brand/model, and
   click Start.
3. Run `CANBusSimulator.Service` (`dotnet run --project src/CANBusSimulator.Service`) with
   `CanReceiver:ComPort` set to the other end (e.g. `COM11`).
4. Point any Modbus TCP client (e.g. `modpoll`, or a few lines against `NModbus`'s
   `ModbusFactory().CreateMaster(tcpClient)`) at `127.0.0.1:5020` (or whatever port you configured)
   and read holding registers starting at 0 - you should see the heartbeat incrementing, link status
   go to 1, and each parameter's status/value registers reflect what the simulator is currently
   transmitting.

## Extending

- **New brand/model**: add one entry to `BrandCatalog.Models` in
  `src/CANBusSimulator.Core/Domain/BrandCatalog.cs`. No other code changes needed.
- **New parameter/PGN**: add one `SimulatedParameter` entry to `ParameterCatalog.All` in
  `src/CANBusSimulator.Core/Domain/ParameterCatalog.cs` with its PGN/SPN, byte offset, length,
  resolution, offset, and physical range. `J1939FrameBuilder`, `J1939FrameDecoder`,
  `SimulationEngine`, and the Modbus `RegisterMap` are all fully generic over `SimulatedParameter`,
  so a new row appears in both the simulator UI and the service's register map automatically. The
  one exception is if a new SPN needs to share a PGN already in use by another simulated parameter -
  that requires grouping parameters by PGN before building/decoding frames (each currently-simulated
  parameter has its own distinct PGN, so this isn't needed today).
- **New CAN transport for the Service**: implement `ICanReceiver`
  (`src/CANBusSimulator.Core/Slcan/ICanReceiver.cs`) and select it in `Worker.OpenReceiver()` - see
  `AdvantechCanReceiver` for the expected shape.

## Known limitations

- **Windows-only UI.** `CANBusSimulator.App` requires the Windows Desktop runtime; `Core`,
  `ModbusSlave`, and `Service` are all cross-platform (only `Service`'s actual Windows Service
  *registration* is Windows-only, not the app itself).
- **Not a hard-real-time CAN traffic generator/receiver.** The simulation timer targets 10-100 Hz,
  which is fine for driving a dashboard or telemetry display, but Windows' default timer resolution
  (~15 ms) means it does not guarantee microsecond-accurate CAN bus timing.
- **No physical hardware in CI/sandboxes.** `SlcanPort`/`SlcanReceiver` are built against
  `ISlcanTransport`/`ISlcanLineSource` seams so their open/write/close/receive sequences are
  unit-tested with fakes; testing against a real USB-CAN adapter (or a virtual COM port pair, e.g.
  [com0com](https://com0com.sourceforge.net/) on Windows, `socat` on Linux) requires that hardware or
  virtual port pair attached - see the end-to-end test recipe above.
- **The Advantech CAN SDK binding is incomplete** - see "The Advantech CAN SDK is not yet wired up"
  above. This is the only piece of the decoder/Modbus service that cannot be finished without the
  vendor's SDK docs/DLL in hand.
