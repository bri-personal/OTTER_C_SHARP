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
        private const UInt32 OPCODE_MASK =      0x0000007F;
        private const UInt32 FUNC3_MASK =       0x00007000;
        private const UInt32 U_IMMED_MASK =     0xFFFFF000;
        private const UInt32 SHIFT_MASK =       0x0000001F;
        private const UInt32 RD_MASK =          0x00000F80;
        private const UInt32 RS1_MASK =         0x000F8000;
        private const UInt32 RS2_MASK =         0x01F00000;
        private const UInt32 MSB7_MASK =        0xFE000000;
        private const UInt32 CSR_MASK =         0xFFF00000;
        private const UInt32 BYTE_MASK =        0x000000FF;
        private const UInt32 HALF_MASK =        0x0000FFFF;
        private const UInt32 BYTE_SIGN_MASK =   0x00000080;
        private const UInt32 B_SIGN_SET_MASK =  0xFFFFFF00;
        private const UInt32 HALF_SIGN_MASK =   0x00008000;
        private const UInt32 H_SIGN_SET_MASK =  0xFFFF0000;

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

        //address constants
        private const UInt32 DATA_ADDR = 0x6000;
        private const UInt32 STACK_ADDR = 0x10000;

        //external IO
        private bool RST, INTR, CLK, IOBUS_WR;
        private Int32 IOBUS_IN, IOBUS_OUT, IOBUS_ADDR;

        //internal components
        private UInt32 pc; //program count
        private UInt32[] regs; //data registers - length 32 array of length 32 BitArrays
        UInt32 ir; //array to hold instruction read from text segment file

        FileStream text; //file for text segment of memory
        BinaryReader textReader; //reader for text segment of memory

        FileStream data; //file for data segment of memory
        BinaryReader dataReader; //reader for data segment of memory
        BinaryWriter dataWriter; //writer for data segment of memory

        public MCU()
        {
            pc = 0; //pc starts at 0

            //initialize registers
            regs = new UInt32[32];
            for (int i = 0; i < 32; i++)
            {
                regs[i] = 0;
            }

            using (text = File.Open("otter_memory.mem", FileMode.Open, FileAccess.Read))
            {
                using (textReader = new BinaryReader(text))
                {
                    using(data=File.Open("data.mem", FileMode.Create, FileAccess.ReadWrite))
                    {
                        using(dataReader= new BinaryReader(data))
                        {
                            using(dataWriter= new BinaryWriter(data))
                            {
                                //fill bits of data segment to 0
                                while(data.Position<STACK_ADDR-DATA_ADDR)
                                {
                                    dataWriter.Write(0);
                                }
                                data.Seek(0, SeekOrigin.Begin); //return position to beginning

                                //read and execute instruction
                                while (pc <= 0x2044)
                                {
                                    Run();
                                }
                                Console.Write(Convert.ToString(pc, 16) + ": END");
                            }
                        }
                    }
                }
            }
        }

        public void Run()
        {
            Console.Write(Convert.ToString(pc, 16) + ": ");
            LoadInstruction();
            Console.Write(Convert.ToString(ir, 16).PadLeft(8, '0')+" ");
            ParseInstruction();
        }

        private void LoadInstruction()
        {
            text.Seek(pc, SeekOrigin.Begin);
            ir = textReader.ReadUInt32();
            pc = (UInt32)text.Seek(0, SeekOrigin.Current);
        }

        private void ParseInstruction()
        {
            switch (ir & OPCODE_MASK)
            {
                case LUI_OPCODE:
                    {
                        //write u immed to rd
                        Console.WriteLine("lui x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_U()>>12, 16));
                        regs[GetRD()]=GenerateImmed_U();
                        break;
                    }
                case AUIPC_OPCODE:
                    {
                        //add u immed to pc and write that to rd
                        Console.WriteLine("auipc x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_U()>>12, 16));
                        regs[GetRD()]= GenerateImmed_U()+pc-4; // -4 to get pc before moving to next instruction
                        break;
                    }
                case JAL_OPCODE:
                    {
                        //write pc+4 to rd and add j immed to pc
                        Console.WriteLine("jal x{0} 0x{1}", GetRD(), Convert.ToString(GenerateImmed_J(), 16));
                        regs[GetRD()] = pc;
                        pc += GenerateImmed_J()-4; //-4 to get pc before moving to next instruction
                        break;
                    }
                case JALR_OPCODE:
                    {
                        //write pc+4 to rd and set pc to value in rs1 + i immed
                        Console.WriteLine("jalr x{0} x{1} 0x{2}", GetRD(), GetRS1(), Convert.ToString(GenerateImmed_I(), 16));
                        regs[GetRD()] = pc;
                        pc = regs[GetRS1()] + GenerateImmed_I();
                        break;
                    }
                case L_OPCODE:
                    {
                        Console.Write("l");
                        data.Seek(GenerateImmed_I()+regs[GetRS1()]-DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
                        UInt32 loadVal = dataReader.ReadUInt32(); //load word from data
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //mask read data to signed byte
                                    Console.Write("b");
                                    loadVal= loadVal & BYTE_MASK;
                                    if ((loadVal & BYTE_SIGN_MASK)!=0)
                                    {
                                        loadVal = loadVal | B_SIGN_SET_MASK;
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //mask read data to signed halfword
                                    Console.Write("h");
                                    loadVal = loadVal & HALF_MASK;
                                    if ((loadVal & HALF_MASK) != 0)
                                    {
                                        loadVal = loadVal | H_SIGN_SET_MASK;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    Console.Write("w");
                                    break;
                                }
                            case 4:
                                {
                                    //mask read data to unsigned byte
                                    Console.Write("bu");
                                    loadVal = loadVal & BYTE_MASK;
                                    break;
                                }
                            case 5:
                                {
                                    //mask read data to unsigned halfword
                                    Console.Write("hu");
                                    loadVal = loadVal & HALF_MASK;
                                    break;
                                }
                            default:
                                {
                                    Console.Write("UNKNOWN");
                                    loadVal = regs[GetRD()]; //so that value in registers doesn't change
                                    break;
                                }
                        }
                        regs[GetRD()] = loadVal; //set value of rd to loaded value
                        Console.WriteLine(" x{0} 0x{1}(x{2})", GetRD(), Convert.ToString(GenerateImmed_I(), 16), GetRS1());
                        break;
                    }
                case I_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //add i immed to value in rs1 and write to rd
                                    Console.Write("addi");
                                    regs[GetRD()] = regs[GetRS1()] + GenerateImmed_I();
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 by 5 LSB of i immed and write to rd
                                    Console.Write("slli");
                                    regs[GetRD()] = regs[GetRS1()] << (Int32) (GenerateImmed_I()&SHIFT_MASK);
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than i immed (signed), 0 otherwise
                                    Console.Write("slti");
                                    regs[GetRD()] = (Int32) regs[GetRS1()] < (Int32) GenerateImmed_I() ? 1u : 0u;
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than i immed (unsigned), 0 otherwise
                                    Console.Write("sltiu");
                                    regs[GetRD()] = regs[GetRS1()] < GenerateImmed_I() ? 1u : 0u;
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and i immed and write to rd
                                    Console.Write("xori");
                                    regs[GetRD()]= regs[GetRS1()]^GenerateImmed_I();
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 by 5 LSB of i immed and write to rd
                                        Console.Write("srli");
                                        regs[GetRD()] = regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK);
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 by 5 LSB of i immed and write to rd
                                        Console.Write("srai");
                                        regs[GetRD()] = (UInt32) ((Int32) regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and i immed and write to rd
                                    Console.Write("ori");
                                    regs[GetRD()] = regs[GetRS1()] | GenerateImmed_I();
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and i immed and write to rd
                                    Console.Write("andi");
                                    regs[GetRD()] = regs[GetRS1()] & GenerateImmed_I();
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
                                    //add b immed to pc if values in rs1 and rs2 are equal
                                    Console.Write("eq");
                                    if (regs[GetRS1()] == regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //add b immed to pc if values in rs1 and rs2 are not equal
                                    Console.Write("ne");
                                    if (regs[GetRS1()] != regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    //add b immed to pc if value in rs1 is less than that in rs2
                                    Console.Write("lt");
                                    if ((Int32)regs[GetRS1()] < (Int32)regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    //add b immed to pc if value in rs1 is greater than or equal that in rs2
                                    Console.Write("ge");
                                    if ((Int32) regs[GetRS1()] >= (Int32) regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //add b immed to pc if value in rs1 is less than that in rs2 (unsigned)
                                    Console.Write("ltu");
                                    if (regs[GetRS1()] < regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 7:
                                {
                                    //add b immed to pc if value in rs1 is greater than or equal that in rs2 (unsigned)
                                    Console.Write("geu");
                                    if (regs[GetRS1()] >= regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
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
                        data.Seek(GenerateImmed_S() + regs[GetRS1()] - DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //store byte (8 bits) in data file
                                    Console.Write("b");
                                    dataWriter.Write((byte)regs[GetRS2()]);
                                    break;
                                }
                            case 1:
                                {
                                    //store halfword (16 bits) in data file
                                    Console.Write("h");
                                    dataWriter.Write((UInt16)regs[GetRS2()]);
                                    break;
                                }
                            case 2:
                                {
                                    //store word (32 bits) in data file
                                    Console.Write("w");
                                    dataWriter.Write(regs[GetRS2()]);
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
                                        //add value in rs1 and value in rs2 and write to rd
                                        Console.Write("add");
                                        regs[GetRD()] = regs[GetRS1()] + regs[GetRS2()];
                                    }
                                    else
                                    {
                                        //subtract value in rs2 from value in rs1 and write to rd
                                        Console.Write("sub");
                                        regs[GetRD()] = regs[GetRS1()] - regs[GetRS2()];
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 and value in rs2 and write to rd
                                    Console.Write("sll");
                                    regs[GetRD()] = regs[GetRS1()] << (Int32)(regs[GetRS2()] & SHIFT_MASK);
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than value in rs2 (signed), 0 otherwise
                                    Console.Write("slt");
                                    regs[GetRD()] = (Int32)regs[GetRS1()] < (Int32) GetRS2() ? 1u : 0u;
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than value in rs2 (unsigned), 0 otherwise
                                    Console.Write("sltu");
                                    regs[GetRD()] = regs[GetRS1()] < regs[GetRS2()] ? 1u : 0u;
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and value in rs2 and write to rd
                                    Console.Write("xor");
                                    regs[GetRD()] = regs[GetRS1()] ^ regs[GetRS2()];
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 and value in rs2 and write to rd
                                        Console.Write("srl");
                                        regs[GetRD()] = regs[GetRS1()] >> (Int32) (regs[GetRS2()] & SHIFT_MASK);
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 and value in rs2 and write to rd
                                        Console.Write("sra");
                                        regs[GetRD()] = (UInt32) ((Int32) regs[GetRS1()] >> (Int32) (regs[GetRS2()] & SHIFT_MASK));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and value in rs2 and write to rd
                                    Console.Write("or");
                                    regs[GetRD()] = regs[GetRS1()] | regs[GetRS2()];
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and value in rs2 and write to rd
                                    Console.Write("and");
                                    regs[GetRD()] = regs[GetRS1()] & regs[GetRS2()];
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

        private UInt32 GetRD()
        {
            return (ir & RD_MASK) >> 7;
        }

        private UInt32 GetRS1()
        {
            return (ir & RS1_MASK) >> 15;
        }

        private UInt32 GetRS2()
        {
            return (ir & RS2_MASK) >> 20;
        }

        private UInt32 GetCSR()
        {
            return (ir & CSR_MASK) >> 20;
        }

        private UInt32 GenerateImmed_I()
        {
            UInt32 imm=0;
            for(int i=0; i<32; i++)
            {
                imm= imm | ( i<11 ? (ir & (1u<<(i+20)))>>20 : (ir& (1u<<31))>>(31-i));
            }

            return imm;
        }

        private UInt32 GenerateImmed_S()
        {
            UInt32 imm = 0;
            for (int i = 0; i < 32; i++)
            {
                imm = imm | (i < 5 ? (ir & (1u << (i + 7))) >> 7 : (i < 11 ? (ir & (1u << (i + 20))) >> 20 : (ir & (1u << 31)) >> (31 - i)));
            }

            return imm;
        }

        private UInt32 GenerateImmed_B()
        {
            UInt32 imm = 0;
            for (int i = 1; i < 11; i++)
            {
                imm = imm | (i < 5 ? (ir & (1u << (i + 7))) >> 7 : (ir & (1u << (i + 20))) >> 20);
            }
            imm = imm | (ir & (1<<7))<<4;
            for(int i=12; i<32; i++)
            {
                imm = imm | ((ir & (1u << 31)) >> (31 - i));
            }

            return imm;
        }

        private UInt32 GenerateImmed_U() //is already left shifted 12 bits
        {
            return ir & U_IMMED_MASK;
        }

        private UInt32 GenerateImmed_J()
        {
            UInt32 imm = 0;
            for(int i=1; i<11; i++)
            {
                imm = imm | ((ir & (1u<<(i+20)))>>20);
            }
            imm = imm | ((ir & (1 << 20))>>9);
            for (int i = 12; i < 32; i++)
            {
                imm = imm | ( i<20 ? (ir & (1u << i)) : (ir&(1u<<31))>>(31-i) );
            }
            return imm;
        }
    }
}