using CANBusSimulator.Core.Domain;
using CANBusSimulator.Core.J1939;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class J1939FrameBuilderTests
{
    private readonly J1939FrameBuilder _builder = new();

    [Fact]
    public void BuildFrame_EngineSpeed_PacksLittleEndianRawValueAtCorrectOffset()
    {
        var def = ParameterCatalog.ByKey(ParameterCatalog.EngineSpeedKey);

        // 1500 rpm / 0.125 rpm-per-bit = 12000 = 0x2EE0
        var frame = _builder.BuildFrame(def, physicalValue: 1500, sourceAddress: 0xF9);

        Assert.Equal(0xE0, frame.Data[3]);
        Assert.Equal(0x2E, frame.Data[4]);
        // Unused bytes must be "not available" (0xFF), not zero.
        Assert.Equal(0xFF, frame.Data[0]);
        Assert.Equal(0xFF, frame.Data[1]);
        Assert.Equal(0xFF, frame.Data[2]);
        Assert.Equal(0xFF, frame.Data[5]);
        Assert.Equal(0xFF, frame.Data[6]);
        Assert.Equal(0xFF, frame.Data[7]);
        Assert.Equal(8, frame.Dlc);
        Assert.True(frame.Extended);
    }

    [Fact]
    public void BuildFrame_EngineSpeed_SetsCorrect29BitCanId()
    {
        var def = ParameterCatalog.ByKey(ParameterCatalog.EngineSpeedKey);

        var frame = _builder.BuildFrame(def, physicalValue: 1500, sourceAddress: 0xF9);

        // priority(3) << 26 | pgn(61444=0xF004) << 8 | sourceAddress(0xF9)
        uint expected = (3u << 26) | (61444u << 8) | 0xF9u;
        Assert.Equal(expected, frame.Id);
    }

    [Fact]
    public void BuildFrame_CoolantTemp_AppliesNegativeOffset()
    {
        var def = ParameterCatalog.ByKey(ParameterCatalog.CoolantTempKey);

        // 90C - (-40) = 130 = 0x82, single byte at offset 0
        var frame = _builder.BuildFrame(def, physicalValue: 90, sourceAddress: 0);

        Assert.Equal(0x82, frame.Data[0]);
    }

    [Fact]
    public void BuildFrame_OilPressure_ScalesBy4KpaPerBit()
    {
        var def = ParameterCatalog.ByKey(ParameterCatalog.OilPressureKey);

        // 400 kPa / 4 kPa-per-bit = 100 = 0x64, at byte offset 3
        var frame = _builder.BuildFrame(def, physicalValue: 400, sourceAddress: 0);

        Assert.Equal(0x64, frame.Data[3]);
    }

    [Fact]
    public void BuildFrame_ClampsOutOfRangeRawValueRatherThanOverflowing()
    {
        var def = ParameterCatalog.ByKey(ParameterCatalog.CoolantTempKey);

        // Physical value far above the encodable raw range must clamp, not wrap/throw.
        var frame = _builder.BuildFrame(def, physicalValue: 10_000, sourceAddress: 0);

        Assert.Equal(0xFE, frame.Data[0]); // maxRaw for a 1-byte field (0xFF reserved as "not available")
    }
}
