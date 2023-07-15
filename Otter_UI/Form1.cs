using Otter;

namespace Otter_UI;

public partial class Form1 : Form
{
    Otter.OtterMCU otter;

    public Form1()
    {
        otter = new Otter.OtterMCU(false, false);
        InitializeComponent();
    }

    //sw# check changed sets corresponding bit of switches MMIO in Otter to 1 or 0
    private void sw0_CheckedChanged(object sender, EventArgs e)
    {
        if (sw0.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000001;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFFE;
        }
    }

    private void sw1_CheckedChanged(object sender, EventArgs e)
    {
        if (sw1.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000002;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFFD;
        }
    }

    private void sw2_CheckedChanged(object sender, EventArgs e)
    {
        if (sw2.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000004;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFFB;
        }
    }

    private void sw3_CheckedChanged(object sender, EventArgs e)
    {
        if (sw3.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000008;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFF7;
        }
    }

    private void sw4_CheckedChanged(object sender, EventArgs e)
    {
        if (sw4.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000010;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFEF;
        }
    }

    private void sw5_CheckedChanged(object sender, EventArgs e)
    {
        if (sw5.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000020;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFDF;
        }
    }

    private void sw6_CheckedChanged(object sender, EventArgs e)
    {
        if (sw6.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000040;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFFBF;
        }
    }

    private void sw7_CheckedChanged(object sender, EventArgs e)
    {
        if (sw7.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000080;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFF7F;
        }
    }

    private void sw8_CheckedChanged(object sender, EventArgs e)
    {
        if (sw8.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000100;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFEFF;
        }
    }

    private void sw9_CheckedChanged(object sender, EventArgs e)
    {
        if (sw9.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000200;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFDFF;
        }
    }

    private void sw10_CheckedChanged(object sender, EventArgs e)
    {
        if (sw10.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000400;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFFBFF;
        }
    }

    private void sw11_CheckedChanged(object sender, EventArgs e)
    {
        if (sw11.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00000800;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFF7FF;
        }
    }

    private void sw12_CheckedChanged(object sender, EventArgs e)
    {
        if (sw12.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00001000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFEFFF;
        }
    }

    private void sw13_CheckedChanged(object sender, EventArgs e)
    {
        if (sw13.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00002000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFDFFF;
        }
    }

    private void sw14_CheckedChanged(object sender, EventArgs e)
    {
        if (sw14.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00004000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFFBFFF;
        }
    }

    private void sw15_CheckedChanged(object sender, EventArgs e)
    {
        if (sw15.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00008000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFF7FFF;
        }
    }

    private void sw16_CheckedChanged(object sender, EventArgs e)
    {
        if (sw16.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00010000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFEFFFF;
        }
    }

    private void sw17_CheckedChanged(object sender, EventArgs e)
    {
        if (sw17.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00020000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFDFFFF;
        }
    }

    private void sw18_CheckedChanged(object sender, EventArgs e)
    {
        if (sw18.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00040000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFFBFFFF;
        }
    }

    private void sw19_CheckedChanged(object sender, EventArgs e)
    {
        if (sw19.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00080000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFF7FFFF;
        }
    }

    private void sw20_CheckedChanged(object sender, EventArgs e)
    {
        if (sw20.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00100000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFEFFFFF;
        }
    }

    private void sw21_CheckedChanged(object sender, EventArgs e)
    {
        if (sw21.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00200000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFDFFFFF;
        }
    }

    private void sw22_CheckedChanged(object sender, EventArgs e)
    {
        if (sw22.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00400000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFFBFFFFF;
        }
    }

    private void sw23_CheckedChanged(object sender, EventArgs e)
    {
        if (sw23.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x00800000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFF7FFFFF;
        }
    }

    private void sw24_CheckedChanged(object sender, EventArgs e)
    {
        if (sw24.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x01000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFEFFFFFF;
        }
    }

    private void sw25_CheckedChanged(object sender, EventArgs e)
    {
        if (sw25.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x02000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFDFFFFFF;
        }
    }

    private void sw26_CheckedChanged(object sender, EventArgs e)
    {
        if (sw26.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x04000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xFBFFFFFF;
        }
    }

    private void sw27_CheckedChanged(object sender, EventArgs e)
    {
        if (sw27.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x08000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xF7FFFFFF;
        }
    }

    private void sw28_CheckedChanged(object sender, EventArgs e)
    {
        if (sw28.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x10000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xEFFFFFFF;
        }
    }

    private void sw29_CheckedChanged(object sender, EventArgs e)
    {
        if (sw29.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x20000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xDFFFFFFF;
        }
    }

    private void sw30_CheckedChanged(object sender, EventArgs e)
    {
        if (sw30.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x40000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0xBFFFFFFF;
        }
    }

    private void sw31_CheckedChanged(object sender, EventArgs e)
    {
        if (sw31.Checked)
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] |= 0x80000000;
        }
        else
        {
            otter.inputTable[Otter.OtterMCU.SW_ADDR] &= 0x7FFFFFFF;
        }
    }
    /////////////////////////////////////////////////////////////////

    //btnC is reset button, sets RST high in otter
    //RST will be set back low when it is handled
    private void btnC_Click(object sender, EventArgs e)
    {
        otter.RST = true;
    }

    //btnL is interrupt button, sets INTR high in otter
    //INTR will be set back low when it is handled
    private void btnL_Click(object sender, EventArgs e)
    {
        otter.INTR = true;
    }

    //every clock tick, run one instruction and update MMIO outputs
    private void clock_Tick(object sender, EventArgs e)
    {
        //run one cycle
        otter.Run();

        //update MMIO
        sevseg.Text = Convert.ToString(otter.outputTable[Otter.OtterMCU.SEVSEG_ADDR],16).ToUpper();

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x1) != 0)
        {
            led0.ForeColor = Color.Lime;
        }
        else
        {
            led0.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x2) != 0)
        {
            led1.ForeColor = Color.Lime;
        }
        else
        {
            led1.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x4) != 0)
        {
            led2.ForeColor = Color.Lime;
        }
        else
        {
            led2.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x8) != 0)
        {
            led3.ForeColor = Color.Lime;
        }
        else 
        { 
            led3.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x10) != 0)
        {
            led4.ForeColor = Color.Lime;
        }
        else 
        { 
            led4.ForeColor = Color.DarkGray;
        }
        
        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x20) != 0)
        {
            led5.ForeColor = Color.Lime;
        }
        else 
        { 
            led5.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x40) != 0)
        {
            led6.ForeColor = Color.Lime;
        }
        else 
        {
            led6.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x80) != 0)
        {
            led7.ForeColor = Color.Lime;
        }
        else 
        { 
            led7.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x100) != 0)
        {
            led8.ForeColor = Color.Lime;
        }
        else 
        {
            led8.ForeColor = Color.DarkGray;
        }
        
        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x200) != 0)
        {
            led9.ForeColor = Color.Lime;
        }
        else
        {
            led9.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x400) != 0)
        {
            led10.ForeColor = Color.Lime;
        }
        else
        {
            led10.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x800) != 0)
        {
            led11.ForeColor = Color.Lime;
        }
        else
        {
            led11.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x1000) != 0)
        {
            led12.ForeColor = Color.Lime;
        }
        else
        {
            led12.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x2000) != 0)
        {
            led13.ForeColor = Color.Lime;
        }
        else
        {
            led13.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x4000) != 0)
        {
            led14.ForeColor = Color.Lime;
        }
        else
        {
            led14.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x8000) != 0)
        {
            led15.ForeColor = Color.Lime;
        }
        else
        {
            led15.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x10000) != 0)
        {
            led16.ForeColor = Color.Lime;
        }
        else
        {
            led16.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x20000) != 0)
        {
            led17.ForeColor = Color.Lime;
        }
        else
        {
            led17.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x40000) != 0)
        {
            led18.ForeColor = Color.Lime;
        }
        else
        {
            led18.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x80000) != 0)
        {
            led19.ForeColor = Color.Lime;
        }
        else
        {
            led19.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x100000) != 0)
        {
            led20.ForeColor = Color.Lime;
        }
        else
        {
            led20.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x200000) != 0)
        {
            led21.ForeColor = Color.Lime;
        }
        else
        {
            led21.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x400000) != 0)
        {
            led22.ForeColor = Color.Lime;
        }
        else
        {
            led22.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x800000) != 0)
        {
            led23.ForeColor = Color.Lime;
        }
        else
        {
            led23.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x1000000) != 0)
        {
            led24.ForeColor = Color.Lime;
        }
        else
        {
            led24.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x2000000) != 0)
        {
            led25.ForeColor = Color.Lime;
        }
        else
        { 
            led25.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x4000000) != 0)
        {
            led26.ForeColor = Color.Lime;
        }
        else
        {
            led26.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x8000000) != 0)
        {
            led27.ForeColor = Color.Lime;
        }
        else
        {
            led27.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x10000000) != 0)
        {
            led28.ForeColor = Color.Lime;
        }
        else
        {
            led28.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x20000000) != 0)
        {
            led29.ForeColor = Color.Lime;
        }
        else
        {
            led29.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x40000000) != 0)
        {
            led30.ForeColor = Color.Lime;
        }
        else
        {
            led30.ForeColor = Color.DarkGray;
        }

        if ((otter.outputTable[Otter.OtterMCU.LED_ADDR] & 0x80000000) != 0)
        {
            led31.ForeColor = Color.Lime;
        }
        else
        {
            led31.ForeColor = Color.DarkGray;
        }
    }

    private void kbDriver_KeyPress(Object sender, KeyPressEventArgs e)
    {
        switch (char.ToUpper(e.KeyChar))
        {
            case (char)Keys.A:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x1C;
                otter.INTR = true;
                break;
            case (char)Keys.B:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x32;
                otter.INTR = true;
                break;
            case (char)Keys.C:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x21;
                otter.INTR = true;
                break;
            case (char)Keys.D:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x23;
                otter.INTR = true;
                break;
            case (char)Keys.E:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x24;
                otter.INTR = true;
                break;
            case (char)Keys.F:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x2B;
                otter.INTR = true;
                break;
            case (char)Keys.G:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x34;
                otter.INTR = true;
                break;
            case (char)Keys.H:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x33;
                otter.INTR = true;
                break;
            case (char)Keys.I:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x43;
                otter.INTR = true;
                break;
            case (char)Keys.J:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x3B;
                otter.INTR = true;
                break;
            case (char)Keys.K:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x42;
                otter.INTR = true;
                break;
            case (char)Keys.L:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x4B;
                otter.INTR = true;
                break;
            case (char)Keys.M:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x3A;
                otter.INTR = true;
                break;
            case (char)Keys.N:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x31;
                otter.INTR = true;
                break;
            case (char)Keys.O:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x44;
                otter.INTR = true;
                break;
            case (char)Keys.P:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x4D;
                otter.INTR = true;
                break;
            case (char)Keys.Q:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x15;
                otter.INTR = true;
                break;
            case (char)Keys.R:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x2D;
                otter.INTR = true;
                break;
            case (char)Keys.S:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x1B;
                otter.INTR = true;
                break;
            case (char)Keys.T:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x2C;
                otter.INTR = true;
                break;
            case (char)Keys.U:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x3C;
                otter.INTR = true;
                break;
            case (char)Keys.V:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x2A;
                otter.INTR = true;
                break;
            case (char)Keys.W:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x1D;
                otter.INTR = true;
                break;
            case (char)Keys.X:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x22;
                otter.INTR = true;
                break;
            case (char)Keys.Y:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x35;
                otter.INTR = true;
                break;
            case (char)Keys.Z:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x1A;
                otter.INTR = true;
                break;
            case (char)Keys.Space:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x29;
                otter.INTR = true;
                break;
            case (char)Keys.Back:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x66;
                otter.INTR = true;
                break;
            case (char)Keys.Enter:
                otter.inputTable[OtterMCU.KB_ADDR] = 0x5A;
                otter.INTR = true;
                break;
            default:
                break;
        }
        e.Handled = true;
    }
}
