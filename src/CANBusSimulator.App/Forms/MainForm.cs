using System.ComponentModel;
using CANBusSimulator.Core.Domain;
using CANBusSimulator.Core.Simulation;
using CANBusSimulator.Core.Slcan;

namespace CANBusSimulator.App.Forms;

public partial class MainForm : Form
{
    private readonly BindingList<ParameterGridRow> _rows = new();
    private SlcanPort? _port;
    private SimulationEngine? _engine;

    public MainForm()
    {
        InitializeComponent();

        SetupParameterGrid();

        // Wire the cascading combo events before populating data, so the initial DataSource
        // assignment's implicit "select index 0" naturally cascades brand -> model -> default ranges.
        _cmbBrand.SelectedIndexChanged += CmbBrand_SelectedIndexChanged;
        _cmbModel.SelectedIndexChanged += CmbModel_SelectedIndexChanged;

        PopulateBrands();
        PopulateConnectionDefaults();
        RefreshPorts();
    }

    private void SetupParameterGrid()
    {
        _dgvParameters.AutoGenerateColumns = false;
        _dgvParameters.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Enabled),
            HeaderText = "On",
            Width = 40,
        });
        _dgvParameters.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Parameter),
            HeaderText = "Parameter",
            ReadOnly = true,
        });
        _dgvParameters.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Min),
            HeaderText = "Min",
        });
        _dgvParameters.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Max),
            HeaderText = "Max",
        });
        _dgvParameters.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Current),
            HeaderText = "Current",
            ReadOnly = true,
        });
        _dgvParameters.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(ParameterGridRow.Unit),
            HeaderText = "Unit",
            ReadOnly = true,
        });

        foreach (var definition in ParameterCatalog.All)
        {
            var state = new ParameterRuntimeState(definition, definition.MinPhysical, definition.MaxPhysical);
            _rows.Add(new ParameterGridRow(state));
        }
        _dgvParameters.DataSource = _rows;
    }

    private void PopulateBrands()
    {
        _cmbBrand.DisplayMember = nameof(Brand.Name);
        _cmbBrand.ValueMember = nameof(Brand.Id);
        _cmbBrand.DataSource = BrandCatalog.Brands.ToList();
    }

    private void CmbBrand_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbBrand.SelectedItem is not Brand brand)
        {
            _cmbModel.DataSource = null;
            return;
        }

        _cmbModel.DisplayMember = nameof(EngineModel.Name);
        _cmbModel.ValueMember = nameof(EngineModel.Id);
        _cmbModel.DataSource = BrandCatalog.ModelsForBrand(brand.Id).ToList();
    }

    private void CmbModel_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_cmbModel.SelectedItem is not EngineModel model)
            return;

        foreach (var row in _rows)
        {
            if (model.DefaultRanges.TryGetValue(row.State.Definition.Key, out var range))
            {
                row.Min = range.DefaultMin;
                row.Max = range.DefaultMax;
            }
        }
    }

    private void PopulateConnectionDefaults()
    {
        _cmbSerialBaud.Items.AddRange(new object[] { "9600", "19200", "38400", "57600", "115200", "230400" });
        _cmbSerialBaud.SelectedItem = "115200";

        foreach (int bps in BaudRateMap.CanBitrateToSlcanCode.Keys.OrderBy(x => x))
            _cmbCanBitrate.Items.Add(bps);
        _cmbCanBitrate.SelectedItem = 250_000; // SAE J1939 standard bus speed
    }

    private void BtnRefreshPorts_Click(object? sender, EventArgs e) => RefreshPorts();

    private void RefreshPorts()
    {
        string? current = _cmbComPort.SelectedItem as string;
        _cmbComPort.Items.Clear();
        _cmbComPort.Items.AddRange(SlcanPort.GetAvailablePorts());
        if (current is not null && _cmbComPort.Items.Contains(current))
            _cmbComPort.SelectedItem = current;
        else if (_cmbComPort.Items.Count > 0)
            _cmbComPort.SelectedIndex = 0;
    }

    private void DgvParameters_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
    {
        string? columnName = _dgvParameters.Columns[e.ColumnIndex].DataPropertyName;
        if (columnName != nameof(ParameterGridRow.Min) && columnName != nameof(ParameterGridRow.Max))
            return;

        var row = _dgvParameters.Rows[e.RowIndex].DataBoundItem as ParameterGridRow;
        if (row is null)
            return;

        if (!double.TryParse(e.FormattedValue?.ToString(), out double value))
        {
            e.Cancel = true;
            _dgvParameters.Rows[e.RowIndex].ErrorText = "Enter a number.";
            return;
        }

        var definition = row.State.Definition;
        if (value < definition.MinPhysical || value > definition.MaxPhysical)
        {
            e.Cancel = true;
            _dgvParameters.Rows[e.RowIndex].ErrorText =
                $"Must be between {definition.MinPhysical:0.###} and {definition.MaxPhysical:0.###}.";
            return;
        }

        bool isMin = columnName == nameof(ParameterGridRow.Min);
        double other = isMin ? row.Max : row.Min;
        if ((isMin && value > other) || (!isMin && value < other))
        {
            e.Cancel = true;
            _dgvParameters.Rows[e.RowIndex].ErrorText = "Min must be less than or equal to Max.";
            return;
        }

        _dgvParameters.Rows[e.RowIndex].ErrorText = string.Empty;
    }

    private void BtnStart_Click(object? sender, EventArgs e)
    {
        if (_cmbComPort.SelectedItem is not string portName)
        {
            MessageBox.Show(this, "Select a COM port first.", "CAN Bus Simulator",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(_cmbSerialBaud.Text, out int serialBaud) || serialBaud <= 0)
        {
            MessageBox.Show(this, "Enter a valid serial baud rate.", "CAN Bus Simulator",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbCanBitrate.SelectedItem is not int canBitrate)
        {
            MessageBox.Show(this, "Select a CAN bitrate.", "CAN Bus Simulator",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        byte sourceAddress = (byte)_numSourceAddress.Value;

        try
        {
            _port = new SlcanPort(portName, serialBaud);
            _port.Open(canBitrate);
        }
        catch (SlcanException ex)
        {
            MessageBox.Show(this, ex.Message, "Could not open COM port",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            _port?.Dispose();
            _port = null;
            return;
        }

        var states = _rows.Select(r => r.State).ToList();
        _engine = new SimulationEngine(_port, states, sourceAddress);
        _engine.FrameSent += Engine_FrameSent;
        _engine.Faulted += Engine_Faulted;
        _engine.Start((int)(1000 / _numUpdateRateHz.Value));

        SetRunningState(true);
        _lblStatus.Text = $"Running on {portName} @ {canBitrate / 1000}kbit/s CAN, {serialBaud} baud serial.";
    }

    private void BtnStop_Click(object? sender, EventArgs e) => StopSimulation();

    private void StopSimulation()
    {
        if (_engine is not null)
        {
            _engine.Stop();
            _engine.FrameSent -= Engine_FrameSent;
            _engine.Faulted -= Engine_Faulted;
            _engine.Dispose();
            _engine = null;
        }

        if (_port is not null)
        {
            _port.Close();
            _port.Dispose();
            _port = null;
        }

        SetRunningState(false);
        _lblStatus.Text = "Idle.";
    }

    private void SetRunningState(bool running)
    {
        _btnStart.Enabled = !running;
        _btnStop.Enabled = running;
        _cmbBrand.Enabled = !running;
        _cmbModel.Enabled = !running;
        _cmbComPort.Enabled = !running;
        _btnRefreshPorts.Enabled = !running;
        _cmbSerialBaud.Enabled = !running;
        _cmbCanBitrate.Enabled = !running;
        _numSourceAddress.Enabled = !running;
        _numUpdateRateHz.Enabled = !running;
    }

    private void Engine_FrameSent(object? sender, SimulationTickEventArgs e)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                AppendLogEntry(e);
                var row = _rows.FirstOrDefault(r => r.State.Definition.Key == e.Parameter.Key);
                row?.RaiseCurrentValueChanged();
            }));
        }
        catch (InvalidOperationException)
        {
            // Form handle is being created/destroyed concurrently with a tick - safe to ignore.
        }
    }

    private void Engine_Faulted(object? sender, Exception ex)
    {
        if (IsDisposed || !IsHandleCreated)
            return;

        try
        {
            BeginInvoke(new Action(() =>
            {
                if (IsDisposed)
                    return;

                StopSimulation();
                MessageBox.Show(this, ex.Message, "Simulation stopped",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }));
        }
        catch (InvalidOperationException)
        {
            // Form handle is being created/destroyed concurrently with a tick - safe to ignore.
        }
    }

    private void AppendLogEntry(SimulationTickEventArgs e)
    {
        string dataHex = string.Join(' ', e.Frame.Data.Take(e.Frame.Dlc).Select(b => b.ToString("X2")));
        var item = new ListViewItem(new[]
        {
            e.Timestamp.ToString("HH:mm:ss.fff"),
            e.Parameter.DisplayName,
            e.Parameter.Pgn.ToString(),
            e.Frame.Id.ToString("X8"),
            e.Frame.Dlc.ToString(),
            dataHex,
            $"{e.Value:0.###} {e.Parameter.Unit}",
        });

        _lvLog.Items.Insert(0, item);

        const int maxRows = 500;
        while (_lvLog.Items.Count > maxRows)
            _lvLog.Items.RemoveAt(_lvLog.Items.Count - 1);
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e) => StopSimulation();
}
