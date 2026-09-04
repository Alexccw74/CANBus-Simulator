using System.ComponentModel;
using System.Runtime.CompilerServices;
using CANBusSimulator.Core.Simulation;

namespace CANBusSimulator.App.Forms;

/// <summary>
/// Data-binding view-model wrapping one <see cref="ParameterRuntimeState"/> row for the parameter
/// DataGridView. Implements <see cref="INotifyPropertyChanged"/> so the grid refreshes automatically
/// when the live current value changes.
/// </summary>
public sealed class ParameterGridRow : INotifyPropertyChanged
{
    public ParameterRuntimeState State { get; }

    public ParameterGridRow(ParameterRuntimeState state) => State = state;

    public bool Enabled
    {
        get => State.Enabled;
        set { State.Enabled = value; OnPropertyChanged(); }
    }

    public string Parameter => State.Definition.DisplayName;

    public double Min
    {
        get => State.UserMin;
        set { State.UserMin = value; OnPropertyChanged(); }
    }

    public double Max
    {
        get => State.UserMax;
        set { State.UserMax = value; OnPropertyChanged(); }
    }

    public double Current => State.CurrentValue;

    public string Unit => State.Definition.Unit;

    /// <summary>Call after <see cref="ParameterRuntimeState.Advance"/> to refresh the bound "Current" cell.</summary>
    public void RaiseCurrentValueChanged() => OnPropertyChanged(nameof(Current));

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
