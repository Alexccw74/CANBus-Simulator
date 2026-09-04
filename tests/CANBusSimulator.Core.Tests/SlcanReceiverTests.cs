using CANBusSimulator.Core.J1939;
using CANBusSimulator.Core.Slcan;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class SlcanReceiverTests
{
    private sealed class FakeLineSource : ISlcanLineSource
    {
        public List<string> Writes { get; } = new();
        public bool IsOpen { get; private set; }

        public event EventHandler<string>? LineReceived;
        public event EventHandler<Exception>? Faulted;

        public void Open() => IsOpen = true;
        public void Write(string text) => Writes.Add(text);
        public void Close() => IsOpen = false;
        public void Dispose() { }

        public void PushLine(string line) => LineReceived?.Invoke(this, line);
        public void PushFault(Exception ex) => Faulted?.Invoke(this, ex);
    }

    [Fact]
    public void Open_SendsBitrateThenOpenChannelCommands()
    {
        var lineSource = new FakeLineSource();
        var receiver = new SlcanReceiver(lineSource);

        receiver.Open(canBitrateBps: 500_000);

        Assert.Equal(new[] { "S6\r", "O\r" }, lineSource.Writes);
    }

    [Fact]
    public void Close_SendsCloseCommandThenClosesLineSource()
    {
        var lineSource = new FakeLineSource();
        var receiver = new SlcanReceiver(lineSource);
        receiver.Open(250_000);

        receiver.Close();

        Assert.Equal("C\r", lineSource.Writes[^1]);
        Assert.False(lineSource.IsOpen);
    }

    [Fact]
    public void ReceivedLine_ThatParsesAsAFrame_RaisesFrameReceived()
    {
        var lineSource = new FakeLineSource();
        var receiver = new SlcanReceiver(lineSource);
        CanFrame? received = null;
        receiver.FrameReceived += (_, e) => received = e.Frame;

        lineSource.PushLine("T18FEF90087DFFFFFFFFFFFFFF");

        Assert.NotNull(received);
        Assert.Equal(0x18FEF900u, received!.Value.Id);
    }

    [Fact]
    public void ReceivedLine_ThatDoesNotParse_DoesNotRaiseFrameReceived()
    {
        var lineSource = new FakeLineSource();
        var receiver = new SlcanReceiver(lineSource);
        bool raised = false;
        receiver.FrameReceived += (_, _) => raised = true;

        lineSource.PushLine("z"); // not a recognized frame-type prefix

        Assert.False(raised);
    }

    [Fact]
    public void LineSourceFault_PropagatesThroughFaultedEvent()
    {
        var lineSource = new FakeLineSource();
        var receiver = new SlcanReceiver(lineSource);
        Exception? caught = null;
        receiver.Faulted += (_, ex) => caught = ex;

        lineSource.PushFault(new IOException("device unplugged"));

        Assert.IsType<IOException>(caught);
    }
}
