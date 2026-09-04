using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class SlcanFrameParserTests
{
    [Fact]
    public void TryParse_ParsesExtendedFrame_MirroringSlcanEncoderTests()
    {
        // Exactly the string SlcanEncoderTests asserts SlcanEncoder.EncodeExtendedFrame produces
        // (terminator already stripped, as a line source would deliver it).
        bool ok = SlcanFrameParser.TryParse("T18FEF90087DFFFFFFFFFFFFFF", out CanFrame frame);

        Assert.True(ok);
        Assert.Equal(0x18FEF900u, frame.Id);
        Assert.True(frame.Extended);
        Assert.Equal(8, frame.Dlc);
        Assert.Equal(new byte[] { 0x7D, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, frame.Data);
    }

    [Fact]
    public void TryParse_RoundTripsSlcanEncoderOutput()
    {
        var original = new CanFrame(
            Id: 0x0CF00400,
            Extended: true,
            Dlc: 8,
            Data: new byte[] { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88 });

        string encoded = SlcanEncoder.EncodeExtendedFrame(original);
        bool ok = SlcanFrameParser.TryParse(encoded.TrimEnd('\r'), out CanFrame decoded);

        Assert.True(ok);
        Assert.Equal(original.Id, decoded.Id);
        Assert.Equal(original.Extended, decoded.Extended);
        Assert.Equal(original.Dlc, decoded.Dlc);
        Assert.Equal(original.Data, decoded.Data);
    }

    [Theory]
    [InlineData("")]
    [InlineData("z123")]
    [InlineData("T123")]
    [InlineData("T1234567")]
    public void TryParse_RejectsMalformedOrNonDataLines(string line)
    {
        bool ok = SlcanFrameParser.TryParse(line, out _);

        Assert.False(ok);
    }
}
