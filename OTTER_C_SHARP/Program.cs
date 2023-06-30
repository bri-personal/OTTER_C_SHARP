using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace Otter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MCU otter = new MCU(); //create OTTER object
        }
    }

    public class MCU
    {
        //bitmask constants
        private const Int32 OPCODE_MASK = 0x7F;
        private const Int32 FUNC3_MASK = 0x00007000;

        //opcode constants
        private const byte LUI_OPCODE = 0x37;
        private const byte AUIPC_OPCODE = 0x17;
        private const byte JAL_OPCODE = 0x6F;
        private const byte JALR_OPCODE = 0x67;
        private const byte L_OPCODE = 0x03;
        private const byte I_OPCODE = 0x13;
        private const byte B_OPCODE = 0x63;
        private const byte S_OPCODE = 0x23;
        private const byte R_OPCODE = 0x33;
        private const byte SYS_OPCODE = 0x73;

        //external IO
        private bool RST, INTR, CLK, IOBUS_WR;
        private Int32 IOBUS_IN, IOBUS_OUT, IOBUS_ADDR;

        //internal components
        private Int32 pc; //program count
        private Int32[] regs; //data registers - length 32 array of length 32 BitArrays
        Int32 ir; //array to hold instruction read from text segment file

        FileStream text; //file for text segment of memory
        BinaryReader textReader; //reader for text segment of memory

        public MCU()
        {
            pc = 0; //pc starts at 0

            //initialize registers
            regs = new Int32[32];
            for (int i = 0; i < 32; i++)
            {
                regs[i] = 0;
            }

            Run();
        }

        public void Run()
        {
            using (text = File.Open("otter_memory.mem", FileMode.Open, FileAccess.Read))
            {
                using (textReader = new BinaryReader(text))
                {
                    while (pc <= 0x2044)
                    {
                        Console.Write(Convert.ToString(pc, 16) + ": ");
                        LoadInstruction();
                        Console.Write(Convert.ToString(ir, 16).PadLeft(8, '0')+" ");
                        ParseInstruction();
                    }
                }
            }
        }

        private void LoadInstruction()
        {
            text.Seek(pc, SeekOrigin.Begin);
            ir = textReader.ReadInt32();
            pc = (Int32)text.Seek(0, SeekOrigin.Current);
        }

        private void ParseInstruction()
        {
            switch (ir & OPCODE_MASK)
            {
                case LUI_OPCODE:
                    {
                        Console.WriteLine("lui");
                        break;
                    }
                case AUIPC_OPCODE:
                    {
                        Console.WriteLine("auipc");
                        break;
                    }
                case JAL_OPCODE:
                    {
                        Console.WriteLine("jal");
                        break;
                    }
                case JALR_OPCODE:
                    {
                        Console.WriteLine("jalr");
                        break;
                    }
                case L_OPCODE:
                    {
                        Console.Write("l");
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("b");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("h");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("w");
                                    break;
                                }
                            case 4:
                                {
                                    Console.WriteLine("bu");
                                    break;
                                }
                            case 5:
                                {
                                    Console.WriteLine("hu");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("UNKNOWN");
                                    break;
                                }
                        }
                        break;
                    }
                case I_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("addi");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("slli");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("slti");
                                    break;
                                }
                            case 3:
                                {
                                    Console.WriteLine("sltiu");
                                    break;
                                }
                            case 4:
                                {
                                    Console.WriteLine("xori");
                                    break;
                                }
                            case 5:
                                {
                                    Console.WriteLine("srli/srai");
                                    break;
                                }
                            case 6:
                                {
                                    Console.WriteLine("ori");
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    Console.WriteLine("andi");
                                    break;
                                }
                        }
                        break;
                    }
                case B_OPCODE:
                    {
                        Console.Write("b");
                        switch((ir&FUNC3_MASK)>>12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("eq");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("ne");
                                    break;
                                }
                            case 4:
                                {
                                    Console.WriteLine("lt");
                                    break;
                                }
                            case 5:
                                {
                                    Console.WriteLine("ge");
                                    break;
                                }
                            case 6:
                                {
                                    Console.WriteLine("ltu");
                                    break;
                                }
                            case 7:
                                {
                                    Console.WriteLine("geu");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("UNKNOWN");
                                    break;
                                }
                        }
                        break;
                    }
                case S_OPCODE:
                    {
                        Console.Write("s");
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("b");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("h");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("w");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("UNKNOWN");
                                    break;
                                }
                        }
                                break;
                    }
                case R_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("add/sub");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("sll");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("slt");
                                    break;
                                }
                            case 3:
                                {
                                    Console.WriteLine("sltu");
                                    break;
                                }
                            case 4:
                                {
                                    Console.WriteLine("xor");
                                    break;
                                }
                            case 5:
                                {
                                    Console.WriteLine("srl/sra");
                                    break;
                                }
                            case 6:
                                {
                                    Console.WriteLine("or");
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    Console.WriteLine("and");
                                    break;
                                }
                        }
                        break;
                    }
                case SYS_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.WriteLine("mret");
                                    break;
                                }
                            case 1:
                                {
                                    Console.WriteLine("csrrw");
                                    break;
                                }
                            case 2:
                                {
                                    Console.WriteLine("csrrs");
                                    break;
                                }
                            case 3:
                                {
                                    Console.WriteLine("csrrc");
                                    break;
                                }
                            default:
                                {
                                    Console.WriteLine("csrUNKNOWN");
                                    break;
                                }
                        }
                        break;
                    }
                default:
                    {
                        Console.WriteLine("UNKNOWN");
                        break;
                    }
            }
        }
    }
}