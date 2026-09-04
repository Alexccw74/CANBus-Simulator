using CANBusSimulator.Core.Simulation;
using Xunit;

namespace CANBusSimulator.Core.Tests;

public class ValueGeneratorTests
{
    [Fact]
    public void Next_AlwaysStaysWithinConfiguredRange()
    {
        var generator = new ValueGenerator(initialValue: 50, rng: new Random(12345));

        for (int i = 0; i < 10_000; i++)
        {
            double value = generator.Next(min: 0, max: 100);
            Assert.InRange(value, 0, 100);
        }
    }

    [Fact]
    public void Next_HandlesInvertedMinMaxBySwapping()
    {
        var generator = new ValueGenerator(initialValue: 50, rng: new Random(1));

        double value = generator.Next(min: 100, max: 0);

        Assert.InRange(value, 0, 100);
    }

    [Fact]
    public void Next_ProducesVaryingValuesRatherThanStayingFlat()
    {
        var generator = new ValueGenerator(initialValue: 0, rng: new Random(7));

        var values = new HashSet<double>();
        for (int i = 0; i < 200; i++)
            values.Add(generator.Next(min: 0, max: 1000));

        Assert.True(values.Count > 1, "Expected the generated value to change over successive calls.");
    }
}
