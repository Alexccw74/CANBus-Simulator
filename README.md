# CAN Bus Simulator

A Windows Forms desktop application that simulates a diesel engine's CAN bus traffic for testing
CAN tooling, dashboards, or telemetry displays without a real engine attached.

You pick an engine **brand and model** (Caterpillar, YANMAR, Cummins, John Deere, Volvo Penta,
Perkins), pick an available **COM port**, set a **min/max range** for each simulated engine
parameter, and start the simulation. The app then streams realistic, slowly-varying values out the
serial port using the **SLCAN (Lawicel) ASCII protocol** - the same wire protocol used by common
USB-CAN adapters (CANable, USBtin, etc.) - encoded as genuine **SAE J1939** frames (Engine Speed,
Coolant Temperature, Oil Pressure, Fuel Level, Battery Voltage, Boost Pressure).

## Project layout

```
CANBusSimulator.sln
src/
  CANBusSimulator.Core/   Portable class library (net8.0, no WinForms dependency):
                          J1939 frame encoding, SLCAN protocol, brand/model catalog, simulation engine.
                          Builds and is unit-tested on any OS.
  CANBusSimulator.App/    Windows Forms UI (net8.0-windows). References Core. Windows-only.
tests/
  CANBusSimulator.Core.Tests/   xUnit tests for Core (frame encoding, SLCAN wire format, value
                                generator bounds, SLCAN port open/close command sequence).
```

The hard, correctness-critical logic (J1939 byte-packing, SLCAN ASCII encoding, bounded random-walk
value generation) lives in `CANBusSimulator.Core`, which has no WinForms dependency and is fully
covered by unit tests that run on Linux, macOS, or Windows. Only the thin `CANBusSimulator.App` UI
project requires Windows to build and run.

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

## Extending

- **New brand/model**: add one entry to `BrandCatalog.Models` in
  `src/CANBusSimulator.Core/Domain/BrandCatalog.cs`. No other code changes needed.
- **New parameter/PGN**: add one `SimulatedParameter` entry to `ParameterCatalog.All` in
  `src/CANBusSimulator.Core/Domain/ParameterCatalog.cs` with its PGN/SPN, byte offset, length,
  resolution, offset, and physical range. `J1939FrameBuilder` and `SimulationEngine` are fully
  generic over `SimulatedParameter`, so a new row appears in the UI automatically. The one exception
  is if a new SPN needs to share a PGN already in use by another simulated parameter - that requires
  grouping parameters by PGN before building frames (each currently-simulated parameter has its own
  distinct PGN, so this isn't needed today).

## Known limitations

- **Windows-only UI.** `CANBusSimulator.App` requires the Windows Desktop runtime; only `Core` is
  cross-platform.
- **Not a hard-real-time CAN traffic generator.** The simulation timer targets 10-100 Hz, which is
  fine for driving a dashboard or telemetry display, but Windows' default timer resolution
  (~15 ms) means it does not guarantee microsecond-accurate CAN bus timing.
- **No physical hardware in CI/sandboxes.** `SlcanPort` is built against an `ISlcanTransport` seam
  so its open/write/close sequence is unit-tested with a fake transport; testing against a real
  USB-CAN adapter (or a virtual COM port pair, e.g. [com0com](https://com0com.sourceforge.net/) on
  Windows) requires a Windows machine with the hardware or virtual port pair attached.
