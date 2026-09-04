using CANBusSimulator.Core.Domain;
using Xunit;

namespace CANBusSimulator.ModbusSlave.Tests;

public class RegisterMapTests
{
    [Fact]
    public void HeartbeatAndLinkStatus_AreTheFirstTwoRegisters()
    {
        Assert.Equal(0, RegisterMap.HeartbeatAddress(registerBase: 0));
        Assert.Equal(1, RegisterMap.LinkStatusAddress(registerBase: 0));
    }

    [Fact]
    public void ParameterRegisters_AreThreeApart_StartingAfterTheGlobalRegisters()
    {
        Assert.Equal(2, RegisterMap.ParameterStatusAddress(0, parameterIndex: 0));
        Assert.Equal(3, RegisterMap.ParameterValueAddress(0, parameterIndex: 0));
        Assert.Equal(5, RegisterMap.ParameterStatusAddress(0, parameterIndex: 1));
        Assert.Equal(6, RegisterMap.ParameterValueAddress(0, parameterIndex: 1));
    }

    [Fact]
    public void Addresses_RespectAConfiguredBase()
    {
        Assert.Equal(100, RegisterMap.HeartbeatAddress(100));
        Assert.Equal(102, RegisterMap.ParameterStatusAddress(100, 0));
    }

    [Fact]
    public void IndexOf_MatchesDeclaredCatalogOrder()
    {
        Assert.Equal(0, RegisterMap.IndexOf(ParameterCatalog.ByKey(ParameterCatalog.EngineSpeedKey)));
        Assert.Equal(5, RegisterMap.IndexOf(ParameterCatalog.ByKey(ParameterCatalog.BoostPressureKey)));
    }

    [Fact]
    public void UpdateParameter_WritesStatusAndFloatValueAtTheExpectedRegisters()
    {
        var service = new ModbusSlaveService(new ModbusSlaveOptions { RegisterBase = 0 });
        var parameter = ParameterCatalog.ByKey(ParameterCatalog.CoolantTempKey);
        int index = RegisterMap.IndexOf(parameter);

        service.UpdateParameter(parameter, 90.0);

        ushort status = service.ReadRegister(RegisterMap.ParameterStatusAddress(0, index));
        ushort valueHi = service.ReadRegister(RegisterMap.ParameterValueAddress(0, index));
        ushort valueLo = service.ReadRegister((ushort)(RegisterMap.ParameterValueAddress(0, index) + 1));

        Assert.Equal(RegisterMap.ParameterStatusValid, status);
        float decoded = ModbusFloatCodec.Decode(valueHi, valueLo, ModbusFloatWordOrder.HighWordFirst);
        Assert.Equal(90.0f, decoded);
    }

    [Fact]
    public void UpdateParameter_WithNullValue_WritesNotAvailableStatus()
    {
        var service = new ModbusSlaveService(new ModbusSlaveOptions());
        var parameter = ParameterCatalog.ByKey(ParameterCatalog.OilPressureKey);
        int index = RegisterMap.IndexOf(parameter);

        service.UpdateParameter(parameter, null);

        ushort status = service.ReadRegister(RegisterMap.ParameterStatusAddress(0, index));
        Assert.Equal(RegisterMap.ParameterStatusNotAvailable, status);
    }

    [Fact]
    public void UpdateParameter_IncrementsHeartbeatAndSetsLinkHealthy()
    {
        var service = new ModbusSlaveService(new ModbusSlaveOptions());
        var parameter = ParameterCatalog.ByKey(ParameterCatalog.FuelLevelKey);

        Assert.Equal(0, service.ReadRegister(RegisterMap.LinkStatusAddress(0)));

        service.UpdateParameter(parameter, 50.0);
        ushort heartbeat1 = service.ReadRegister(RegisterMap.HeartbeatAddress(0));

        service.UpdateParameter(parameter, 51.0);
        ushort heartbeat2 = service.ReadRegister(RegisterMap.HeartbeatAddress(0));

        Assert.Equal(1, service.ReadRegister(RegisterMap.LinkStatusAddress(0)));
        Assert.Equal((ushort)(heartbeat1 + 1), heartbeat2);
    }

    [Fact]
    public void UpdateParameter_RespectsLowWordFirstOption()
    {
        var service = new ModbusSlaveService(new ModbusSlaveOptions { FloatWordOrder = ModbusFloatWordOrder.LowWordFirst });
        var parameter = ParameterCatalog.ByKey(ParameterCatalog.BatteryVoltageKey);
        int index = RegisterMap.IndexOf(parameter);

        service.UpdateParameter(parameter, 24.0);

        ushort first = service.ReadRegister(RegisterMap.ParameterValueAddress(0, index));
        ushort second = service.ReadRegister((ushort)(RegisterMap.ParameterValueAddress(0, index) + 1));
        float decoded = ModbusFloatCodec.Decode(first, second, ModbusFloatWordOrder.LowWordFirst);

        Assert.Equal(24.0f, decoded);
    }
}
