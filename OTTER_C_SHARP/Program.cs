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
        private const UInt32 OPCODE_MASK =  0x0000007F;
        private const UInt32 FUNC3_MASK =   0x00007000;
        private const UInt32 U_IMMED_MASK = 0xFFFFF000;
        private const UInt32 RD_MASK =      0x00000F80;
        private const UInt32 RS1_MASK =     0x000F8000;
        private const UInt32 RS2_MASK =     0x01F00000;
        private const UInt32 MSB7_MASK =    0xFE000000;
        private const UInt32 CSR_MASK =     0xFFF00000;

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

        FileStream? text; //file for text segment of memory
        BinaryReader? textReader; //reader for text segment of memory

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
            text!.Seek(pc, SeekOrigin.Begin);
            ir = textReader!.ReadInt32();
            pc = (Int32)text.Seek(0, SeekOrigin.Current);
        }

        private void ParseInstruction()
        {
            switch (ir & OPCODE_MASK)
            {
                case LUI_OPCODE:
                    {
                        Console.WriteLine("lui x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_U()>>12, 16));
                        break;
                    }
                case AUIPC_OPCODE:
                    {
                        Console.WriteLine("auipc x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_U()>>12, 16));
                        break;
                    }
                case JAL_OPCODE:
                    {
                        Console.WriteLine("jal x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_J(), 16));
                        break;
                    }
                case JALR_OPCODE:
                    {
                        Console.WriteLine("jalr x{0} x{1} 0x{2}", GetRD(), GetRS1(), Convert.ToString(GenerateImmed_I(), 16));
                        break;
                    }
                case L_OPCODE:
                    {
                        Console.Write("l");
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.Write("b");
                                    break;
                                }
                            case 1:
                                {
                                    Console.Write("h");
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("w");
                                    break;
                                }
                            case 4:
                                {
                                    Console.Write("bu");
                                    break;
                                }
                            case 5:
                                {
                                    Console.Write("hu");
                                    break;
                                }
                            default:
                                {
                                    Console.Write("UNKNOWN");
                                    break;
                                }
                        }
                        Console.WriteLine(" x{0} 0x{1}(x{2})", GetRD(), Convert.ToString(GenerateImmed_I(), 16), GetRS1());
                        break;
                    }
                case I_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.Write("addi");
                                    break;
                                }
                            case 1:
                                {
                                    Console.Write("slli");
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("slti");
                                    break;
                                }
                            case 3:
                                {
                                    Console.Write("sltiu");
                                    break;
                                }
                            case 4:
                                {
                                    Console.Write("xori");
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        Console.Write("srli");
                                    }
                                    else
                                    {
                                        Console.Write("srai");
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    Console.Write("ori");
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    Console.Write("andi");
                                    break;
                                }
                        }
                        Console.WriteLine(" x{0} x{1} 0x{2}", GetRD(), GetRS1(), Convert.ToString(GenerateImmed_I(), 16));
                        break;
                    }
                case B_OPCODE:
                    {
                        Console.Write("b");
                        switch((ir&FUNC3_MASK)>>12)
                        {
                            case 0:
                                {
                                    Console.Write("eq");
                                    break;
                                }
                            case 1:
                                {
                                    Console.Write("ne");
                                    break;
                                }
                            case 4:
                                {
                                    Console.Write("lt");
                                    break;
                                }
                            case 5:
                                {
                                    Console.Write("ge");
                                    break;
                                }
                            case 6:
                                {
                                    Console.Write("ltu");
                                    break;
                                }
                            case 7:
                                {
                                    Console.Write("geu");
                                    break;
                                }
                            default:
                                {
                                    Console.Write("UNKNOWN");
                                    break;
                                }
                        }
                        Console.WriteLine(" x{0} x{1} 0x{2}", GetRS1(), GetRS2(), Convert.ToString(GenerateImmed_B(), 16));
                        break;
                    }
                case S_OPCODE:
                    {
                        Console.Write("s");
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    Console.Write("b");
                                    break;
                                }
                            case 1:
                                {
                                    Console.Write("h");
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("w");
                                    break;
                                }
                            default:
                                {
                                    Console.Write("UNKNOWN");
                                    break;
                                }
                        }
                        Console.WriteLine(" x{0} 0x{1}(x{2})", GetRS2(), Convert.ToString(GenerateImmed_S(), 16), GetRS1());
                        break;
                    }
                case R_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    if((ir&MSB7_MASK)==0)
                                    {
                                        Console.Write("add");
                                    }
                                    else
                                    {
                                        Console.Write("sub");
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    Console.Write("sll");
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("slt");
                                    break;
                                }
                            case 3:
                                {
                                    Console.Write("sltu");
                                    break;
                                }
                            case 4:
                                {
                                    Console.Write("xor");
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        Console.Write("srl");
                                    }
                                    else
                                    {
                                        Console.Write("sra");
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    Console.Write("or");
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    Console.Write("and");
                                    break;
                                }
                        }
                        Console.WriteLine(" x{0} x{1} x{2}", GetRD(), GetRS1(), GetRS2());
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
                                    Console.Write("csrrw");
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("csrrs");
                                    break;
                                }
                            case 3:
                                {
                                    Console.Write("csrrc");
                                    break;
                                }
                            default:
                                {
                                    Console.Write("csrUNKNOWN");
                                    break;
                                }
                        }
                        if((ir & FUNC3_MASK)!=0)
                        {
                            Console.WriteLine(" x{0} 0x{1} x{2}", GetRD(), Convert.ToString(GetCSR(), 16), GetRS1());
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

        private Int32 GetRD()
        {
            return (Int32) (ir & RD_MASK) >> 7;
        }

        private Int32 GetRS1()
        {
            return (Int32)(ir & RS1_MASK) >> 15;
        }

        private Int32 GetRS2()
        {
            return (Int32)(ir & RS2_MASK) >> 20;
        }

        private Int32 GetCSR()
        {
            return (Int32)(ir & CSR_MASK) >> 20;
        }

        private Int32 GenerateImmed_I()
        {
            Int32 imm=0;
            for(int i=0; i<32; i++)
            {
                imm= imm | ( i<11 ? (ir & (1<<(i+20)))>>20 : (ir& (1<<31))>>(31-i));
            }

            return imm;
        }

        private Int32 GenerateImmed_S()
        {
            Int32 imm = 0;
            for (int i = 0; i < 32; i++)
            {
                imm = imm | (i < 5 ? (ir & (1 << (i + 7))) >> 7 : (i < 11 ? (ir & (1 << (i + 20))) >> 20 : (ir & (1 << 31)) >> (31 - i)));
            }

            return imm;
        }

        private Int32 GenerateImmed_B()
        {
            Int32 imm = 0;
            for (int i = 1; i < 11; i++)
            {
                imm = imm | (i < 5 ? (ir & (1 << (i + 7))) >> 7 : (ir & (1 << (i + 20))) >> 20);
            }
            imm = imm | (ir & (1<<7))<<4;
            for(int i=12; i<32; i++)
            {
                imm = imm | ((ir & (1 << 31)) >> (31 - i));
            }

            return imm;
        }

        private Int32 GenerateImmed_U() //is already left shifted 12 bits
        {
            return (Int32) (ir & U_IMMED_MASK);
        }

        private Int32 GenerateImmed_J() //BAD (adds 1 somehow)
        {
            Int32 imm = 0;
            for(int i=1; i<11; i++)
            {
                imm = imm | ((ir & (1<<(i+20)))>>20);
            }
            imm = imm | ((ir & (1 << 20))>>9);
            for (int i = 12; i < 32; i++)
            {
                imm = imm | ( i<20 ? (ir & (1 << i)) : (ir&(1<<31))>>(31-i) );
            }
            return imm;
        }
    }
}