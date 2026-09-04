using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class SlcanPortTests
{
    private sealed class FakeTransport : ISlcanTransport
    {
        public List<string> Writes { get; } = new();
        public bool IsOpen { get; private set; }
        public bool OpenCalled { get; private set; }
        public bool CloseCalled { get; private set; }
        public bool ThrowUnauthorizedOnOpen { get; set; }

        public void Open()
        {
            OpenCalled = true;
            if (ThrowUnauthorizedOnOpen)
                throw new UnauthorizedAccessException("Access denied.");
            IsOpen = true;
        }

        public void Write(string text) => Writes.Add(text);

        public void Close()
        {
            CloseCalled = true;
            IsOpen = false;
        }

        public void Dispose() { }
    }

    [Fact]
    public void Open_SendsBitrateThenOpenChannelCommands()
    {
        var transport = new FakeTransport();
        var port = new SlcanPort(transport);

        port.Open(canBitrateBps: 500_000);

        Assert.Equal(new[] { "S6\r", "O\r" }, transport.Writes);
    }

    [Fact]
    public void Open_TranslatesUnauthorizedAccessIntoSlcanException()
    {
        var transport = new FakeTransport { ThrowUnauthorizedOnOpen = true };
        var port = new SlcanPort(transport);

        var ex = Assert.Throws<SlcanException>(() => port.Open(500_000));
        Assert.Contains("already in use", ex.Message);
    }

    [Fact]
    public void SendFrame_WritesEncodedFrameAfterOpen()
    {
        var transport = new FakeTransport();
        var port = new SlcanPort(transport);
        port.Open(250_000);

        var frame = new CanFrame(0x18FEF900, Extended: true, Dlc: 1, Data: new byte[] { 0x42 });
        port.SendFrame(frame);

        Assert.Equal("T18FEF900142\r", transport.Writes[^1]);
    }

    [Fact]
    public void Close_SendsCloseCommandThenClosesTransport()
    {
        var transport = new FakeTransport();
        var port = new SlcanPort(transport);
        port.Open(250_000);

        port.Close();

        Assert.Equal("C\r", transport.Writes[^1]);
        Assert.True(transport.CloseCalled);
    }

    [Fact]
    public void SendFrame_BeforeOpen_ThrowsSlcanException()
    {
        var transport = new FakeTransport();
        var port = new SlcanPort(transport);

        var frame = new CanFrame(0x18FEF900, Extended: true, Dlc: 1, Data: new byte[] { 0x42 });
        Assert.Throws<SlcanException>(() => port.SendFrame(frame));
    }
}
