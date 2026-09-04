using CANBusSimulator.Core.Domain;
using CANBusSimulator.Core.J1939;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class J1939FrameDecoderTests
{
    private readonly J1939FrameBuilder _builder = new();
    private readonly J1939FrameDecoder _decoder = new();

    [Theory]
    [InlineData(ParameterCatalog.EngineSpeedKey, 1500.0)]
    [InlineData(ParameterCatalog.CoolantTempKey, 90.0)]
    [InlineData(ParameterCatalog.OilPressureKey, 400.0)]
    [InlineData(ParameterCatalog.FuelLevelKey, 60.0)]
    [InlineData(ParameterCatalog.BatteryVoltageKey, 24.0)]
    [InlineData(ParameterCatalog.BoostPressureKey, 150.0)]
    public void Decode_RoundTripsEveryEncodedParameter(string key, double physicalValue)
    {
        var definition = ParameterCatalog.ByKey(key);
        var frame = _builder.BuildFrame(definition, physicalValue, sourceAddress: 0xF9);

        var decoded = _decoder.Decode(frame);

        var match = Assert.Single(decoded, d => d.Parameter.Key == key);
        Assert.NotNull(match.Value);
        Assert.Equal(physicalValue, match.Value!.Value, precision: 1);
        Assert.Equal(0xF9, match.SourceAddress);
    }

    [Fact]
    public void Decode_OneByteNotAvailableSentinel_YieldsNullValue()
    {
        var definition = ParameterCatalog.ByKey(ParameterCatalog.CoolantTempKey);
        var data = new byte[8];
        Array.Fill(data, (byte)0xFF); // SAE "not available" for every byte, including this SPN's

        uint id = J1939Id.Build(definition.DefaultPriority, definition.Pgn, sourceAddress: 0);
        var frame = new CanFrame(id, Extended: true, Dlc: 8, Data: data);

        var decoded = _decoder.Decode(frame);

        var match = Assert.Single(decoded);
        Assert.Null(match.Value);
    }

    [Fact]
    public void Decode_TwoByteNotAvailableSentinel_YieldsNullValue()
    {
        var definition = ParameterCatalog.ByKey(ParameterCatalog.EngineSpeedKey);
        var data = new byte[8];
        Array.Fill(data, (byte)0xFF);

        uint id = J1939Id.Build(definition.DefaultPriority, definition.Pgn, sourceAddress: 0);
        var frame = new CanFrame(id, Extended: true, Dlc: 8, Data: data);

        var decoded = _decoder.Decode(frame);

        var match = Assert.Single(decoded);
        Assert.Null(match.Value);
    }

    [Fact]
    public void Decode_UnrecognizedPgn_YieldsNoMatches()
    {
        var frame = new CanFrame(
            J1939Id.Build(priority: 6, pgn: 0x1234, sourceAddress: 0),
            Extended: true, Dlc: 8, Data: new byte[8]);

        var decoded = _decoder.Decode(frame);

        Assert.Empty(decoded);
    }

    [Fact]
    public void Decode_TruncatedFrame_SkipsParameterItCannotCarry()
    {
        var definition = ParameterCatalog.ByKey(ParameterCatalog.EngineSpeedKey); // needs bytes 3-4
        uint id = J1939Id.Build(definition.DefaultPriority, definition.Pgn, sourceAddress: 0);
        var frame = new CanFrame(id, Extended: true, Dlc: 3, Data: new byte[8]); // too short

        var decoded = _decoder.Decode(frame);

        Assert.Empty(decoded);
    }
}
