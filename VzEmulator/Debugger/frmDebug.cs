using Konamiman.Z80dotNet;
using System;
using System.Windows.Forms;

namespace VzEmulator
{
    public partial class frmDebug : Form
    {
        private Z80Processor _cpu;
        private MemUtils _memUtils;
        private Timer _timer;

        private TextboxValidation validation = new TextboxValidation();

        const int RegisterLabelColumn = 0;
        const int RegisterValueLabelColumn = 1;

        const int AddressLabelColumn = 1;
        const int ValueTextBoxColumn = 2;

        internal frmDebug(Z80Processor cpu, MemUtils memUtils)
        {
            InitializeComponent();
            _cpu = cpu;
            _memUtils = memUtils;

            SetupMemoryPanelEvents();
            SetupRegisterPanelEvents();
            InitialiseTimer();

        }
        private void SetupMemoryPanelEvents()
        {
            for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
            {
                var textBoxControl = tableLayoutPanel1.GetControlFromPosition(ValueTextBoxColumn, row) as TextBox;
                if (textBoxControl != null)
                {
                    textBoxControl.Leave += ValueTextBox_Leave;
                    textBoxControl.Validating += validation.ValueTextBox_Validate;
                }
                textBoxControl = tableLayoutPanel1.GetControlFromPosition(AddressLabelColumn, row) as TextBox;
                if (textBoxControl != null)
                {
                    textBoxControl.Validating += validation.ValueTextBox_Validate;
                }
            }
        }
        private void SetupRegisterPanelEvents()
        {
            for (int row = 0; row < tableLayoutPanel3.RowCount; row++)
            {
                var textBoxControl = tableLayoutPanel3.GetControlFromPosition(RegisterValueLabelColumn, row) as TextBox;
                if (textBoxControl != null)
                {
                    textBoxControl.Leave += RegisterValueTextBox_Leave;
                    textBoxControl.Validating += validation.ValueTextBox_Validate;
                }
            }
        }
        private void InitialiseTimer()
        {
            _timer = new Timer();
            _timer.Interval = 10; //ms
            _timer.Tick += (v, a) => {
                UpdateDebugInfo();
            };
            _timer.Start();
        }

        private void UpdateDebugInfo()
        {
            for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
            {
                var labelControl = tableLayoutPanel1.GetControlFromPosition(AddressLabelColumn, row) as Control;
                var textBoxControl = tableLayoutPanel1.GetControlFromPosition(ValueTextBoxColumn, row) as TextBox;
                if (labelControl != null && textBoxControl != null && (!textBoxControl.Focused || textBoxControl.Text.Length==0))
                {
                    try
                    {
                        var address = MemUtils.StringToUShort(labelControl.Text);
                        var value = _memUtils.GetWordAtAddress(address);
                        textBoxControl.Text = string.Format("0x{0:X4}", value);
                    }
                    catch (Exception) { /* do nothing */ }
                }
            }

            SetValueText(txtAF, _cpu.Registers.AF);
            SetValueText(txtBC, _cpu.Registers.BC);
            SetValueText(txtDE, _cpu.Registers.DE);
            SetValueText(txtHL, _cpu.Registers.HL);
            SetValueText(txtPC, _cpu.Registers.PC);
            SetValueText(txtSP, _cpu.Registers.SP);
            SetValueText(txtIX, _cpu.Registers.IX);
            SetValueText(txtIY, _cpu.Registers.IY);
            SetValueTextBin(txtFlags, _cpu.Registers.F);
        }
        private void SetValueText(TextBox textBoxControl, short value)
        {
            if (!textBoxControl.Focused || textBoxControl.Text.Length==0)
            {
                textBoxControl.Text = string.Format("0x{0:X4}", value);
            }
        }
        private void SetValueText(TextBox textBoxControl, ushort value)
        {
            if (!textBoxControl.Focused || textBoxControl.Text.Length == 0)
            {
                textBoxControl.Text = string.Format("0x{0:X4}", value);
            }
        }
        private void SetValueTextBin(TextBox textBoxControl, byte value)
        {
            if (!textBoxControl.Focused || textBoxControl.Text.Length == 0)
            {
                var binValue = Convert.ToString(value, 2);
                binValue = binValue.PadLeft(8, '0');
                textBoxControl.Text = binValue;
            }
        }
        private void ValueTextBox_Leave(object sender, System.EventArgs e)
        {
            var ctrl = sender as Control;
            if (ctrl != null)
            {

                var labelControl = tableLayoutPanel1.GetControlFromPosition(AddressLabelColumn, tableLayoutPanel1.GetPositionFromControl(ctrl).Row) as Control;
                if (labelControl != null)
                {
                    try
                    {
                        var address = MemUtils.StringToUShort(labelControl.Text);
                        var value = MemUtils.StringToUShort(ctrl.Text);
                        _memUtils.SetWordAtAddress(address, value);
                    }
                    catch (Exception )
                    {
                        //do nothing
                    }
                }
            }
        }
        private void RegisterValueTextBox_Leave(object sender, EventArgs e)
        {
            var ctrl = sender as Control;
            if (ctrl != null)
            {

                var labelControl = tableLayoutPanel3.GetControlFromPosition(RegisterLabelColumn, tableLayoutPanel3.GetPositionFromControl(ctrl).Row) as Control;
                if (labelControl != null)
                {
                    try
                    {
                        var register = labelControl.Text;
                        var value = MemUtils.StringToShort(ctrl.Text);
                        SetRegisterValue(register, value);
                    }
                    catch (FormatException)
                    {
                        //do nothing
                    }
                }
            }
        }

        private void SetRegisterValue(string register, short value)
        {
            switch (register)
            {
                case "AF":
                    _cpu.Registers.AF = value;
                    break;
                case "BC":
                    _cpu.Registers.BC = value;
                    break;
                case "DE":
                    _cpu.Registers.DE = value;
                    break;
                case "HL":
                    _cpu.Registers.HL = value;
                    break;
                case "IX":
                    _cpu.Registers.IX = value;
                    break;
                case "IY":
                    _cpu.Registers.IY = value;
                    break;
                case "SP":
                    _cpu.Registers.SP = value;
                    break;
                case "PC":
                    unsafe
                    {
                        _cpu.Registers.PC = (ushort)value;
                    }
                    break;


            }
        }
        

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label25_Click(object sender, EventArgs e)
        {

        }
    }
    }
