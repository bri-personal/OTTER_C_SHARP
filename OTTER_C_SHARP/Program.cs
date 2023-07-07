using System.Collections;

namespace Otter
{
    //FIX: storing 0 to leds every time

    public class Program
    {
        public static void Main(string[] args)
        {
            OtterMCU otter = new OtterMCU(); //create OTTER object
        }
    }

    public class OtterMCU
    {
        //specific register names array matches indices to names
        public static string[] REG_NAMES = new string[32] 
            { "zero", 
                "ra", 
                "sp", 
                "gp", 
                "tp", 
                "t0", 
                "t1", 
                "t2", 
                "s0", 
                "s1", 
                "a0", 
                "a1", 
                "a2", 
                "a3", 
                "a4", 
                "a5", 
                "a6", 
                "a7", 
                "s2", 
                "s3", 
                "s4", 
                "s5", 
                "s6", 
                "s7", 
                "s8", 
                "s9", 
                "s10", 
                "s11", 
                "t3", 
                "t4", 
                "t5", 
                "t6" };

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
        private const UInt32 MMIO_ADDR = 0x11000000;
        private const UInt32 SW_ADDR = MMIO_ADDR;
        private const UInt32 LED_ADDR = MMIO_ADDR+0x20;
        private const UInt32 SEVSEG_ADDR = MMIO_ADDR + 0x40;
        private const UInt32 VGA_PIXEL_ADDR = MMIO_ADDR + 0x120;
        private const UInt32 VGA_COLOR_ADDR = MMIO_ADDR + 0x140;


        //external IO
        public bool RST, INTR, CLK, IOBUS_WR;
        public UInt32 IOBUS_IN, IOBUS_OUT, IOBUS_ADDR;

        //internal components
        public Dictionary<UInt32, UInt32> inputTable, outputTable; //MMIO addresses/values
        public UInt32 pc; //program count
        private UInt32[] regs; //data registers - length 32 array of 32 bit ints
        private UInt32 ir; //32 bit int to hold instruction read from text segment file
        private UInt32 mtvec; //32 bit int to hold address of ISR for CSR
        private UInt32 mepc; //32 bit int to hold return address when interrupt triggered
        private UInt32 mstatus; //32 bit int to hold MIE and MPIE bits for enabling interrupts

        private FileStream text; //file for text segment of memory
        private BinaryReader textReader; //reader for text segment of memory

        private FileStream data; //file for data segment of memory
        private BinaryReader dataReader; //reader for data segment of memory
        private BinaryWriter dataWriter; //writer for data segment of memory

        public bool showInstr, debug; //flags to show verbose output or not
        public bool wrongEndian; //flag to reverse bytes when loading from text segment

        public OtterMCU()
        {
            //set output flags
            showInstr = true;
            debug = true;
            wrongEndian = true;

            //initialize MMIO addresses/values
            inputTable = new Dictionary<UInt32, UInt32>(1);
            inputTable.Add(SW_ADDR, 2); //b010 for switches -> pause if test fails but not after every test

            outputTable = new Dictionary<UInt32, UInt32>(2);
            outputTable.Add(LED_ADDR, 0);
            outputTable.Add(SEVSEG_ADDR, 0);
            outputTable.Add(VGA_PIXEL_ADDR, 0);
            outputTable.Add(VGA_COLOR_ADDR, 0);

            pc = 0; //pc starts at 0
            ir = mtvec = mepc = mstatus = 0; //initialize other vars

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

                                //read and execute instructions
                                while (pc <= 0x3f5c)
                                {
                                    Run();
                                    if(pc==0x22c)
                                    {
                                        Console.WriteLine("FAIL");
                                        break;
                                    }
                                }
                                Console.Write(Convert.ToString(pc, 16) + ": END");
                            }
                        }
                    }
                }
            }
        }

        //runs through cycle for one instruction
        public void Run()
        {
            Console.Write(Convert.ToString(pc, 16) + ": ");
            LoadInstruction();
            Console.Write(Convert.ToString(ir, 16).PadLeft(8, '0')+" ");
            ParseInstruction();
        }

        //load instruction from binary mem file into ir and set pc to pc+4
        private void LoadInstruction()
        {
            text.Seek(pc, SeekOrigin.Begin);
            ir = textReader.ReadUInt32();
            if(wrongEndian)
            {
                ir = ReverseBytes(ir); //if endianness is wrong
            }
            
            pc = (UInt32)text.Seek(0, SeekOrigin.Current);
        }

        //parse quantities from instruction and perform operations and store result
        private void ParseInstruction()
        {
            switch (ir & OPCODE_MASK)
            {
                case LUI_OPCODE:
                    {
                        //write u immed to rd
                        setRD(GenerateImmed_U());
                        if (showInstr)
                        {
                            Console.WriteLine("lui {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_U() >> 12, 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("register {0} now contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }
                        break;
                    }
                case AUIPC_OPCODE:
                    {
                        //add u immed to pc and write that to rd
                        setRD(GenerateImmed_U()+pc-4); // -4 to get pc before moving to next instruction
                        if (debug)
                        {
                            Console.WriteLine("auipc {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_U() >> 12, 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("register {0} now contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }
                        break;
                    }
                case JAL_OPCODE:
                    {
                        //write pc+4 to rd and add j immed to pc
                        setRD(pc);
                        pc += GenerateImmed_J()-4; //-4 to get pc before moving to next instruction
                        if (showInstr)
                        {
                            Console.WriteLine("jal {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_J(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("register {0} is now 0x{1} and pc is now 0x{2}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()],16), Convert.ToString(pc, 16));
                        }
                        break;
                    }
                case JALR_OPCODE:
                    {
                        //write pc+4 to rd and set pc to value in rs1 + i immed
                        UInt32 tmp = pc;
                        pc = regs[GetRS1()] + GenerateImmed_I();
                        setRD(tmp);
                        if (showInstr)
                        {
                            Console.WriteLine("jalr {0} {1} 0x{2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], Convert.ToString(GenerateImmed_I(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("register {0} is now 0x{1} and pc is now 0x{2}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16), Convert.ToString(pc, 16));
                        }
                        break;
                    }
                case L_OPCODE:
                    {
                        if(showInstr)
                        {
                            Console.Write("l");
                        }

                        UInt32 offset = GenerateImmed_I() + regs[GetRS1()];
                        UInt32 loadVal;

                        if(offset<DATA_ADDR) //loading from text segment - good assembly program shouldn't do this but it is possible
                        {
                            text.Seek(offset, SeekOrigin.Begin); //move text filestream to given offset
                            loadVal = textReader.ReadUInt32(); //load word from text

                            if(wrongEndian)
                            {
                                loadVal = ReverseBytes(loadVal); //if endianness is wrong
                            }
                        }
                        else if (offset<STACK_ADDR) //loading from data segment
                        {
                            data.Seek(offset - DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
                            loadVal = dataReader.ReadUInt32(); //load word from data
                        }
                        else if(offset>=MMIO_ADDR) //loading from MMIO
                        {
                            if(!inputTable.TryGetValue(offset, out loadVal)) //load word from data
                            {
                                //unimplemented MMIO address
                                throw new IOException($"MMIO address {offset} is not connected to an input device");
                            }
                        }
                        else //reserved memory
                        {
                            throw new Exception($"Cannot load from address 0x{Convert.ToString(offset, 16)} in reserved memory");
                        }

                        //if necessary, mask loaded data
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //mask read data to signed byte
                                    Console.Write("b");
                                    loadVal = loadVal & BYTE_MASK;
                                    if ((loadVal & BYTE_SIGN_MASK) != 0)
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
                                    if ((loadVal & HALF_SIGN_MASK) != 0)
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
                                    //unknown func3
                                    throw new Exception("Instruction func3 does not correspond to any known instruction");
                                }
                        }
                        setRD(loadVal); //set value of rd to loaded value

                        if(showInstr)
                        {
                            Console.WriteLine(" {0} 0x{1}({2})", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_I(), 16), REG_NAMES[GetRS1()]);
                        }
                        if(debug)
                        {
                            Console.WriteLine("loaded from address {0}", Convert.ToString(offset, 16));
                            Console.WriteLine("rd {0} contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }
                        
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
                                    setRD(regs[GetRS1()] + GenerateImmed_I());
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 by 5 LSB of i immed and write to rd
                                    Console.Write("slli");
                                    setRD(regs[GetRS1()] << (Int32) (GenerateImmed_I()&SHIFT_MASK));
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than i immed (signed), 0 otherwise
                                    Console.Write("slti");
                                    setRD((Int32) regs[GetRS1()] < (Int32) GenerateImmed_I() ? 1u : 0u);
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than i immed (unsigned), 0 otherwise
                                    Console.Write("sltiu");
                                    setRD(regs[GetRS1()] < GenerateImmed_I() ? 1u : 0u);
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and i immed and write to rd
                                    Console.Write("xori");
                                    setRD(regs[GetRS1()]^GenerateImmed_I());
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 by 5 LSB of i immed and write to rd
                                        Console.Write("srli");
                                        setRD(regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK));
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 by 5 LSB of i immed and write to rd
                                        Console.Write("srai");
                                        setRD((UInt32) ((Int32) regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK)));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and i immed and write to rd
                                    Console.Write("ori");
                                    setRD(regs[GetRS1()] | GenerateImmed_I());
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and i immed and write to rd
                                    Console.Write("andi");
                                    setRD(regs[GetRS1()] & GenerateImmed_I());
                                    break;
                                }
                        }
                        Console.WriteLine(" {0} {1} 0x{2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], Convert.ToString(GenerateImmed_I(), 16));
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
                                    //unknown func3
                                    throw new Exception("Instruction func3 does not correspond to any known instruction");
                                }
                        }
                        Console.WriteLine(" {0} {1} 0x{2}", REG_NAMES[GetRS1()], REG_NAMES[GetRS2()], Convert.ToString(GenerateImmed_B(), 16));
                        break;
                    }
                case S_OPCODE:
                    {
                        Console.Write("s");

                        UInt32 offset = GenerateImmed_S() + regs[GetRS1()];
                        if(offset<DATA_ADDR) //storing to text segment - SHOULD NOT HAPPEN
                        {
                            throw new Exception($"Cannot store to address 0x{Convert.ToString(offset,16)} in text segment");
                        }
                        else if (offset<STACK_ADDR) //storing to data segment
                        {
                            data.Seek(offset - DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
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
                                        //unknown func3
                                        throw new Exception("Instruction func3 does not correspond to any known instruction");
                                    }
                            }
                        }
                        else if(offset >=MMIO_ADDR) //storing to MMIO
                        {
                            switch ((ir & FUNC3_MASK) >> 12)
                            {
                                case 0:
                                    {
                                        //store byte (8 bits) to MMIO
                                        Console.Write("b");
                                        if(outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = (byte)regs[GetRS2()];
                                            //Console.WriteLine($"{Convert.ToString(offset,16)}: {outputTable[offset]}");
                                        }
                                        else
                                        {
                                            //unimplemented MMIO address
                                            throw new IOException($"MMIO address {offset} is not connected to an output device");
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        //store halfword (16 bits) to MMIO
                                        Console.Write("h");
                                        if (outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = (UInt16)regs[GetRS2()];
                                            //Console.WriteLine($"{Convert.ToString(offset, 16)}: {outputTable[offset]}");
                                        }
                                        else
                                        {
                                            //unimplemented MMIO address
                                            throw new IOException($"MMIO address {offset} is not connected to an output device");
                                        }
                                        break;
                                    }
                                case 2:
                                    {
                                        //store word (32 bits) to MMIO
                                        Console.Write("w");
                                        if (outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = regs[GetRS2()];
                                            //Console.WriteLine($"{Convert.ToString(offset, 16)}: {outputTable[offset]}");
                                        }
                                        else
                                        {
                                            //unimplemented MMIO address
                                            throw new IOException($"MMIO address {offset} is not connected to an output device");
                                        }
                                        break;
                                    }
                                default:
                                    {
                                        //unknown func3
                                        throw new Exception($"Cannot store to address 0x{Convert.ToString(offset, 16)} in reserved memory");
                                    }
                            }
                        }
                        else //reserved memory
                        {
                            throw new Exception("Cannot store to reserved memory");
                        }
                        Console.WriteLine(" {0} 0x{1}({2})", REG_NAMES[GetRS2()], Convert.ToString(GenerateImmed_S(), 16), REG_NAMES[GetRS1()]);
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
                                        setRD(regs[GetRS1()] + regs[GetRS2()]);
                                    }
                                    else
                                    {
                                        //subtract value in rs2 from value in rs1 and write to rd
                                        Console.Write("sub");
                                        setRD(regs[GetRS1()] - regs[GetRS2()]);
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 and value in rs2 and write to rd
                                    Console.Write("sll");
                                    setRD(regs[GetRS1()] << (Int32)(regs[GetRS2()] & SHIFT_MASK));
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than value in rs2 (signed), 0 otherwise
                                    setRD((Int32)regs[GetRS1()] < (Int32)regs[GetRS2()] ? 1u : 0u);
                                    if (showInstr)
                                    {
                                        Console.Write("slt");
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than value in rs2 (unsigned), 0 otherwise
                                    Console.Write("sltu");
                                    setRD(regs[GetRS1()] < regs[GetRS2()] ? 1u : 0u);
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and value in rs2 and write to rd
                                    Console.Write("xor");
                                    setRD(regs[GetRS1()] ^ regs[GetRS2()]);
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 and value in rs2 and write to rd
                                        Console.Write("srl");
                                        setRD(regs[GetRS1()] >> (Int32) (regs[GetRS2()] & SHIFT_MASK));
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 and value in rs2 and write to rd
                                        Console.Write("sra");
                                        setRD((UInt32)((Int32)regs[GetRS1()] >> (Int32)(regs[GetRS2()] & SHIFT_MASK)));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and value in rs2 and write to rd
                                    Console.Write("or");
                                    setRD(regs[GetRS1()] | regs[GetRS2()]);
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and value in rs2 and write to rd
                                    Console.Write("and");
                                    setRD(regs[GetRS1()] & regs[GetRS2()]);
                                    break;
                                }
                        }
                        if(showInstr)
                        {
                            Console.WriteLine(" {0} {1} {2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], REG_NAMES[GetRS2()]);
                        }
                        if (debug)
                        {
                            Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                            Console.WriteLine("rs2 {0} contains 0x{1}", REG_NAMES[GetRS2()], Convert.ToString(regs[GetRS2()], 16));
                            Console.WriteLine("rd {0} contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }
                        break;
                    }
                case SYS_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //return execution to where it left off before interrupt, addr given by mepc
                                    Console.WriteLine("mret");
                                    pc = mepc;
                                    break;
                                }
                            case 1:
                                {
                                    //read csr value into rd and write value of rs1 into csr
                                    Console.Write("csrrw");
                                    UInt32 csr = GetCSR();
                                    if(csr==0x341) //mepc
                                    {
                                        setRD(mepc);
                                        mepc = regs[GetRS1()];
                                    }
                                    else if (csr == 0x305) //mtvec
                                    {
                                        setRD(mtvec);
                                        mtvec = regs[GetRS1()];
                                    }
                                    else if (csr == 0x300) //mstatus
                                    {
                                        setRD(mstatus);
                                        mstatus = regs[GetRS1()];
                                    }
                                    else //unimplmented register
                                    {
                                        throw new NotImplementedException($"CSR register {csr} not implemented");
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    //read csr value into rd and set bits of csr corresponding to 1s of value of rs1
                                    Console.Write("csrrs");
                                    UInt32 csr = GetCSR();
                                    if (csr == 0x341) //mepc
                                    {
                                        setRD(mepc);
                                        mepc |= regs[GetRS1()];
                                    }
                                    else if (csr == 0x305) //mtvec
                                    {
                                        setRD(mtvec);
                                        mtvec |= regs[GetRS1()];
                                    }
                                    else if (csr == 0x300) //mstatus
                                    {
                                        setRD(mstatus);
                                        mstatus |= regs[GetRS1()];
                                    }
                                    else //unimplmented register
                                    {
                                        throw new NotImplementedException($"CSR register {csr} not implemented");
                                    }
                                    break;
                                }
                            case 3:
                                {
                                    //read csr value into rd and clear bits of csr corresponding to 1s of value of rs1
                                    Console.Write("csrrc");
                                    UInt32 csr = GetCSR();
                                    if (csr == 0x341) //mepc
                                    {
                                        setRD(mepc);
                                        mepc &= ~regs[GetRS1()];
                                    }
                                    else if (csr == 0x305) //mtvec
                                    {
                                        setRD(mtvec);
                                        mtvec &= ~regs[GetRS1()];
                                    }
                                    else if (csr == 0x300) //mstatus
                                    {
                                        setRD(mstatus);
                                        mstatus &= ~regs[GetRS1()];
                                    }
                                    else //unimplmented register
                                    {
                                        throw new NotImplementedException($"CSR register {csr} not implemented");
                                    }
                                    break;
                                }
                            default:
                                {
                                    //unknown func3
                                    throw new Exception("Instruction func3 does not correspond to any known instruction");
                                }
                        }
                        if((ir & FUNC3_MASK)!=0)
                        {
                            Console.WriteLine(" {0} 0x{1} {2}", REG_NAMES[GetRD()], Convert.ToString(GetCSR(), 16), REG_NAMES[GetRS1()]);
                        }
                        break;
                    }
                default:
                    {
                        //unknown opcode
                        throw new Exception("Instruction opcode does not correspond to any known instruction");
                    }
            }
        }

        //set rd to given value. ir must already contain current instruction
        private void setRD(UInt32 value)
        {
            UInt32 rd=GetRD();
            if(rd>0)
            {
                regs[rd] = value;
            }
        }

        //get destination register from instruction
        private UInt32 GetRD()
        {
            return (ir & RD_MASK) >> 7;
        }

        //get source register 1 from instruction
        private UInt32 GetRS1()
        {
            return (ir & RS1_MASK) >> 15;
        }

        //get source register 2 from instruction
        private UInt32 GetRS2()
        {
            return (ir & RS2_MASK) >> 20;
        }

        //get CSR address from instruction
        private UInt32 GetCSR()
        {
            return (ir & CSR_MASK) >> 20;
        }

        //generate I type immediate from instruction
        private UInt32 GenerateImmed_I()
        {
            UInt32 imm=0;
            for(int i=0; i<32; i++)
            {
                imm= imm | ( i<11 ? (ir & (1u<<(i+20)))>>20 : (ir& (1u<<31))>>(31-i));
            }

            return imm;
        }

        //generate S type immediate from instruction
        private UInt32 GenerateImmed_S()
        {
            UInt32 imm = 0;
            for (int i = 0; i < 32; i++)
            {
                imm = imm | (i < 5 ? (ir & (1u << (i + 7))) >> 7 : (i < 11 ? (ir & (1u << (i + 20))) >> 20 : (ir & (1u << 31)) >> (31 - i)));
            }

            return imm;
        }

        //generate B type immediate from instruction
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

        //generate U type immediate from instruction
        private UInt32 GenerateImmed_U() //is already left shifted 12 bits
        {
            return ir & U_IMMED_MASK;
        }

        //generate J type immediate from instruction
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

        // reverse byte order (32-bit)
        private static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                   (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }
    }
}