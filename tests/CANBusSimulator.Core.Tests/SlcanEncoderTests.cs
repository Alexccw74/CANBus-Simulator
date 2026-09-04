using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class SlcanEncoderTests
{
    [Fact]
    public void EncodeExtendedFrame_ProducesExactLawicelAsciiString()
    {
        var frame = new CanFrame(
            Id: 0x18FEF900,
            Extended: true,
            Dlc: 8,
            Data: new byte[] { 0x7D, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });

        string encoded = SlcanEncoder.EncodeExtendedFrame(frame);

        Assert.Equal("T18FEF90087DFFFFFFFFFFFFFF\r", encoded);
    }

    [Fact]
    public void EncodeExtendedFrame_RejectsStandardFrames()
    {
        var frame = new CanFrame(Id: 0x123, Extended: false, Dlc: 8, Data: new byte[8]);

        Assert.Throws<ArgumentException>(() => SlcanEncoder.EncodeExtendedFrame(frame));
    }

    [Fact]
    public void EncodeSetBitrate_WrapsCodeInSCommand()
    {
        Assert.Equal("S6\r", SlcanEncoder.EncodeSetBitrate('6'));
    }

    [Fact]
    public void BaudRateMap_MapsStandardJ1939BitrateTo250k()
    {
        char code = BaudRateMap.CodeFor(250_000);
        Assert.Equal('5', code);
    }
}
