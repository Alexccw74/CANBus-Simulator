namespace CANBusSimulator.App.Forms;

partial class MainForm
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
            _engine?.Dispose();
            _port?.Dispose();
        }
        base.Dispose(disposing);
    }

    private GroupBox _grpEngine = null!;
    private Label _lblBrand = null!;
    private ComboBox _cmbBrand = null!;
    private Label _lblModel = null!;
    private ComboBox _cmbModel = null!;

    private GroupBox _grpConnection = null!;
    private Label _lblComPort = null!;
    private ComboBox _cmbComPort = null!;
    private Button _btnRefreshPorts = null!;
    private Label _lblSerialBaud = null!;
    private ComboBox _cmbSerialBaud = null!;
    private Label _lblCanBitrate = null!;
    private ComboBox _cmbCanBitrate = null!;
    private Label _lblSourceAddress = null!;
    private NumericUpDown _numSourceAddress = null!;

    private Label _lblUpdateRate = null!;
    private NumericUpDown _numUpdateRateHz = null!;
    private Button _btnStart = null!;
    private Button _btnStop = null!;

    private SplitContainer _splitMain = null!;
    private DataGridView _dgvParameters = null!;
    private ListView _lvLog = null!;

    private StatusStrip _statusStrip = null!;
    private ToolStripStatusLabel _lblStatus = null!;

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();

        _grpEngine = new GroupBox();
        _lblBrand = new Label();
        _cmbBrand = new ComboBox();
        _lblModel = new Label();
        _cmbModel = new ComboBox();

        _grpConnection = new GroupBox();
        _lblComPort = new Label();
        _cmbComPort = new ComboBox();
        _btnRefreshPorts = new Button();
        _lblSerialBaud = new Label();
        _cmbSerialBaud = new ComboBox();
        _lblCanBitrate = new Label();
        _cmbCanBitrate = new ComboBox();
        _lblSourceAddress = new Label();
        _numSourceAddress = new NumericUpDown();

        _lblUpdateRate = new Label();
        _numUpdateRateHz = new NumericUpDown();
        _btnStart = new Button();
        _btnStop = new Button();

        _splitMain = new SplitContainer();
        _dgvParameters = new DataGridView();
        _lvLog = new ListView();

        _statusStrip = new StatusStrip();
        _lblStatus = new ToolStripStatusLabel();

        SuspendLayout();

        // _grpEngine
        _grpEngine.Text = "Engine";
        _grpEngine.Location = new Point(12, 12);
        _grpEngine.Size = new Size(360, 90);
        _lblBrand.Text = "Brand:";
        _lblBrand.Location = new Point(12, 28);
        _lblBrand.AutoSize = true;
        _cmbBrand.Location = new Point(90, 24);
        _cmbBrand.Size = new Size(240, 23);
        _cmbBrand.DropDownStyle = ComboBoxStyle.DropDownList;
        _lblModel.Text = "Model:";
        _lblModel.Location = new Point(12, 58);
        _lblModel.AutoSize = true;
        _cmbModel.Location = new Point(90, 54);
        _cmbModel.Size = new Size(240, 23);
        _cmbModel.DropDownStyle = ComboBoxStyle.DropDownList;
        _grpEngine.Controls.Add(_lblBrand);
        _grpEngine.Controls.Add(_cmbBrand);
        _grpEngine.Controls.Add(_lblModel);
        _grpEngine.Controls.Add(_cmbModel);

        // _grpConnection
        _grpConnection.Text = "Connection";
        _grpConnection.Location = new Point(384, 12);
        _grpConnection.Size = new Size(430, 150);
        _lblComPort.Text = "COM Port:";
        _lblComPort.Location = new Point(12, 28);
        _lblComPort.AutoSize = true;
        _cmbComPort.Location = new Point(100, 24);
        _cmbComPort.Size = new Size(150, 23);
        _cmbComPort.DropDownStyle = ComboBoxStyle.DropDownList;
        _btnRefreshPorts.Text = "Refresh";
        _btnRefreshPorts.Location = new Point(260, 23);
        _btnRefreshPorts.Size = new Size(75, 25);
        _btnRefreshPorts.Click += BtnRefreshPorts_Click;
        _lblSerialBaud.Text = "Serial Baud:";
        _lblSerialBaud.Location = new Point(12, 58);
        _lblSerialBaud.AutoSize = true;
        _cmbSerialBaud.Location = new Point(100, 54);
        _cmbSerialBaud.Size = new Size(150, 23);
        _cmbSerialBaud.DropDownStyle = ComboBoxStyle.DropDown;
        _lblCanBitrate.Text = "CAN Bitrate:";
        _lblCanBitrate.Location = new Point(12, 88);
        _lblCanBitrate.AutoSize = true;
        _cmbCanBitrate.Location = new Point(100, 84);
        _cmbCanBitrate.Size = new Size(150, 23);
        _cmbCanBitrate.DropDownStyle = ComboBoxStyle.DropDownList;
        _lblSourceAddress.Text = "Source Addr (hex):";
        _lblSourceAddress.Location = new Point(12, 118);
        _lblSourceAddress.AutoSize = true;
        _numSourceAddress.Location = new Point(140, 114);
        _numSourceAddress.Size = new Size(80, 23);
        _numSourceAddress.Minimum = 0;
        _numSourceAddress.Maximum = 255;
        _numSourceAddress.Hexadecimal = true;
        _numSourceAddress.Value = 0xF9; // conventional "off-board diagnostic tool" address
        _grpConnection.Controls.Add(_lblComPort);
        _grpConnection.Controls.Add(_cmbComPort);
        _grpConnection.Controls.Add(_btnRefreshPorts);
        _grpConnection.Controls.Add(_lblSerialBaud);
        _grpConnection.Controls.Add(_cmbSerialBaud);
        _grpConnection.Controls.Add(_lblCanBitrate);
        _grpConnection.Controls.Add(_cmbCanBitrate);
        _grpConnection.Controls.Add(_lblSourceAddress);
        _grpConnection.Controls.Add(_numSourceAddress);

        // rate + start/stop row
        _lblUpdateRate.Text = "Update Rate (Hz):";
        _lblUpdateRate.Location = new Point(12, 172);
        _lblUpdateRate.AutoSize = true;
        _numUpdateRateHz.Location = new Point(140, 168);
        _numUpdateRateHz.Size = new Size(60, 23);
        _numUpdateRateHz.Minimum = 1;
        _numUpdateRateHz.Maximum = 50;
        _numUpdateRateHz.Value = 10;
        _btnStart.Text = "Start";
        _btnStart.Location = new Point(650, 166);
        _btnStart.Size = new Size(80, 28);
        _btnStart.Click += BtnStart_Click;
        _btnStop.Text = "Stop";
        _btnStop.Location = new Point(736, 166);
        _btnStop.Size = new Size(80, 28);
        _btnStop.Enabled = false;
        _btnStop.Click += BtnStop_Click;

        // _dgvParameters
        _dgvParameters.Dock = DockStyle.Fill;
        _dgvParameters.AllowUserToAddRows = false;
        _dgvParameters.AllowUserToDeleteRows = false;
        _dgvParameters.RowHeadersVisible = false;
        _dgvParameters.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _dgvParameters.CellValidating += DgvParameters_CellValidating;

        // _lvLog
        _lvLog.Dock = DockStyle.Fill;
        _lvLog.View = View.Details;
        _lvLog.FullRowSelect = true;
        _lvLog.GridLines = true;
        _lvLog.Columns.Add("Time", 90);
        _lvLog.Columns.Add("Parameter", 130);
        _lvLog.Columns.Add("PGN", 60);
        _lvLog.Columns.Add("CAN ID", 90);
        _lvLog.Columns.Add("DLC", 40);
        _lvLog.Columns.Add("Data", 170);
        _lvLog.Columns.Add("Value", 110);

        // _splitMain
        _splitMain.Location = new Point(12, 205);
        _splitMain.Size = new Size(802, 400);
        _splitMain.Orientation = Orientation.Horizontal;
        _splitMain.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        _splitMain.SplitterDistance = 200;
        _splitMain.Panel1.Controls.Add(_dgvParameters);
        _splitMain.Panel2.Controls.Add(_lvLog);

        // status strip
        _statusStrip.Items.Add(_lblStatus);
        _lblStatus.Text = "Idle.";

        // MainForm
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(826, 640);
        Controls.Add(_grpEngine);
        Controls.Add(_grpConnection);
        Controls.Add(_lblUpdateRate);
        Controls.Add(_numUpdateRateHz);
        Controls.Add(_btnStart);
        Controls.Add(_btnStop);
        Controls.Add(_splitMain);
        Controls.Add(_statusStrip);
        MinimumSize = new Size(700, 500);
        Text = "CAN Bus Simulator";
        FormClosing += MainForm_FormClosing;
        ResumeLayout(false);
        PerformLayout();
    }
}
