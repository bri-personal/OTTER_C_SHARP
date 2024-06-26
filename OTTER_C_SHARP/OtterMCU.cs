﻿namespace Otter
{
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
        private const UInt32 INTR_MASK =        0xFFFFFF77;
        private const UInt32 HEX_MASK =         0x00000008;

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
        public const UInt32 SW_ADDR = MMIO_ADDR;
        public const UInt32 RAND_ADDR = MMIO_ADDR + 0x60;
        public const UInt32 KB_ADDR = MMIO_ADDR + 0x100;
        public const UInt32 LED_ADDR = MMIO_ADDR + 0x20;
        public const UInt32 SEVSEG_ADDR = MMIO_ADDR + 0x40;
        public const UInt32 SPK_ADDR = MMIO_ADDR + 0x80;
        public const UInt32 VGA_PIXEL_ADDR = MMIO_ADDR + 0x120;
        public const UInt32 VGA_COLOR_ADDR = MMIO_ADDR + 0x140;
        public const UInt32 VGA_READ_ADDR = MMIO_ADDR + 0x160;


        //external IO
        public bool RST, INTR;

        //internal components
        public Dictionary<UInt32, UInt32> inputTable, outputTable; //MMIO addresses/values
        private UInt32 pc; //program count
        private UInt32[] regs; //data registers - length 32 array of 32 bit ints
        private UInt32 ir; //32 bit int to hold instruction read from text segment file
        private UInt32 mtvec; //32 bit int to hold address of ISR for CSR
        private UInt32 mepc; //32 bit int to hold return address when interrupt triggered
        private UInt32 mstatus; //32 bit int to hold MIE and MPIE bits for enabling interrupts

        private FileStream? text; //file for text segment of memory
        private BinaryReader? textReader; //reader for text segment of memory
        private BinaryWriter? textWriter; //writer for text segment of memory

        private FileStream? data; //file for data segment of memory
        private BinaryReader? dataReader; //reader for data segment of memory
        private BinaryWriter? dataWriter; //writer for data segment of memory

        public byte[,] vgaBuffer; //2D array for vga pixels, can be read from and written to

        public bool showInstr, debug; //flags to show verbose output or not

        public static void Main(string[] args)
        {
            OtterMCU otter = new OtterMCU(false, true);
            otter.StartInConsole();
            //HexReverser.ReverseHex();
        }

        public OtterMCU(bool showInstr, bool debug)
        {
            //set output flags
            this.showInstr = showInstr;
            this.debug = debug;

            //initialize MMIO addresses/values
            inputTable = new Dictionary<UInt32, UInt32>()
            {
                { SW_ADDR, 0 },
                { RAND_ADDR, 0 },
                { KB_ADDR, 0 },
                { VGA_READ_ADDR, 0 }
            };

            outputTable = new Dictionary<UInt32, UInt32>()
            {
                { LED_ADDR, 0 },
                { SEVSEG_ADDR, 0 },
                { SPK_ADDR, 0 },
                { VGA_PIXEL_ADDR, 0 },
                { VGA_COLOR_ADDR, 0 }
            };

            pc = 0; //pc starts at 0
            ir = mtvec = mepc = mstatus = 0; //initialize other vars

            //initialize registers
            regs = new UInt32[32];
            for (int i = 0; i < 32; i++)
            {
                regs[i] = 0;
            }

            //create 2D array for vga buffer pixels
            vgaBuffer = new byte[60, 80];
        }

        public void StartInConsole()
        {
            using(text = File.Open("otter_memory.mem", FileMode.Open, FileAccess.ReadWrite))
            {
                using(textReader = new BinaryReader(text))
                {
                    using(textWriter = new BinaryWriter(text))
                    {
                        using(data = File.Open("data.mem", FileMode.Create, FileAccess.ReadWrite))
                        {
                            using(dataReader = new BinaryReader(data))
                            {
                                using(dataWriter = new BinaryWriter(data))
                                {
                                    //fill remaining bits of memory files
                                    fillMemory();

                                    //read and execute instructions
                                    while (true)
                                    {
                                        Run(); //read and execute one instruction

                                        //check for special addresses (for debugging only)
                                        if (pc == mtvec)
                                        {
                                            Console.WriteLine("ISR");
                                        }

                                        //check for reset or interrupt
                                        string? input = Console.ReadLine();
                                        if (input is not null)
                                        {
                                            if (input.Equals("R"))
                                            {
                                                RST = true;
                                            }
                                            else if (input.Equals("I"))
                                            {
                                                INTR = true;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //Infinite loop of running otter with no console IO
        //To be called by UI
        public void StartNoConsole()
        {
            using (text = File.Open("otter_memory.mem", FileMode.Open, FileAccess.ReadWrite))
            {
                using (textReader = new BinaryReader(text))
                {
                    using (textWriter = new BinaryWriter(text))
                    {
                        using (data = File.Open("data.mem", FileMode.Create, FileAccess.ReadWrite))
                        {
                            using (dataReader = new BinaryReader(data))
                            {
                                using (dataWriter = new BinaryWriter(data))
                                {
                                    //fill remaining bits of memory files
                                    fillMemory();

                                    while (true)
                                    {
                                        Run();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        //fill extra bits of memory files
        private void fillMemory()
        {
            //fill remaining bits of text segment to 0
            text!.Seek(0, SeekOrigin.End);
            while (text.Position < DATA_ADDR)
            {
                textWriter!.Write(0);
            }
            text.Seek(0, SeekOrigin.Begin); //return position to beginning

            //fill bits of data segment to 0
            while (data!.Position < STACK_ADDR - DATA_ADDR)
            {
                dataWriter!.Write(0);
            }
            data.Seek(0, SeekOrigin.Begin); //return position to beginning
        }

        //runs through cycle for one instruction
        public void Run()
        {
            if (RST) //if RST high, reset pc and csr registers
            {
                pc = 0;
                mtvec = mepc = mstatus = 0;
                RST = false; //set RST flag back to false
            }
            else if (INTR)
            {
                if ((mstatus & HEX_MASK)!= 0) //if INTR high and interrupts enabled, go to interrupt state
                {
                    Interrupt();
                }
                INTR = false; //set INTR flag back to false
            }

            if (showInstr || debug)
            {
                Console.Write(Convert.ToString(pc, 16) + ": ");
            }

            LoadInstruction();

            if (showInstr || debug)
            {
                Console.Write(Convert.ToString(ir, 16).PadLeft(8, '0') + " ");
            }

            ParseInstruction();
        }

        //interrupt sequence
        private void Interrupt()
        {
            mstatus = (mstatus & INTR_MASK)|((mstatus&HEX_MASK)<<4); //copy mie bit to mpie bit and clear mie bit
            mepc = pc; //set mepc to current pc
            pc = mtvec; //set pc to mtvec
        }

        //load instruction from binary mem file into ir and set pc to pc+4
        private void LoadInstruction()
        {
            text!.Seek(pc, SeekOrigin.Begin);
            ir = textReader!.ReadUInt32();
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

                        if (showInstr || debug)
                        {
                            Console.WriteLine("lui {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_U() >> 12, 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rd {0} contains 0x{1}\n", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }

                        break;
                    }
                case AUIPC_OPCODE:
                    {
                        //add u immed to pc and write that to rd
                        setRD(GenerateImmed_U() + pc - 4); // -4 to get pc before moving to next instruction

                        if (showInstr || debug)
                        {
                            Console.WriteLine("auipc {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_U() >> 12, 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rd {0} contains 0x{1}\n", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }

                        break;
                    }
                case JAL_OPCODE:
                    {
                        //write pc+4 to rd and add j immed to pc
                        setRD(pc);
                        pc += GenerateImmed_J() - 4; //-4 to get pc before moving to next instruction

                        if (showInstr || debug)
                        {
                            Console.WriteLine("jal {0} 0x{1}", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_J(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rd {0} contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                            Console.WriteLine("pc is now 0x{0}\n", Convert.ToString(pc, 16));
                        }

                        break;
                    }
                case JALR_OPCODE:
                    {
                        //write pc+4 to rd and set pc to value in rs1 + i immed
                        UInt32 tmp = pc;
                        pc = regs[GetRS1()] + GenerateImmed_I();
                        setRD(tmp);

                        if (showInstr || debug)
                        {
                            Console.WriteLine("jalr {0} {1} 0x{2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], Convert.ToString(GenerateImmed_I(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                            Console.WriteLine("rd {0} is now 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                            Console.WriteLine("pc is now 0x{0}\n", Convert.ToString(pc, 16));
                        }

                        break;
                    }
                case L_OPCODE:
                    {
                        UInt32 offset = GenerateImmed_I() + regs[GetRS1()];
                        UInt32 loadVal;

                        if (offset < DATA_ADDR) //loading from text segment - good assembly program shouldn't do this but it is possible
                        {
                            text!.Seek(offset, SeekOrigin.Begin); //move text filestream to given offset
                            loadVal = textReader!.ReadUInt32(); //load word from text
                        }
                        else if (offset < STACK_ADDR) //loading from data segment
                        {
                            data!.Seek(offset - DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
                            loadVal = dataReader!.ReadUInt32(); //load word from data
                        }
                        else if (offset >= MMIO_ADDR) //loading from MMIO
                        {
                            //get random number if needed
                            if(offset == RAND_ADDR)
                            {
                                Random r = new Random();
                                inputTable[RAND_ADDR]=(UInt32) r.Next(Int32.MinValue, Int32.MaxValue);
                            }
                            //read vga buffer if needed
                            else if (offset == VGA_READ_ADDR)
                            {
                                inputTable[VGA_READ_ADDR]=vgaBuffer[outputTable[VGA_PIXEL_ADDR] / 128, outputTable[VGA_PIXEL_ADDR] % 128];
                            }
                            Console.WriteLine("offset:"+Convert.ToString(offset,16));
                            if (!inputTable.TryGetValue(offset, out loadVal)) //load word from data
                            {
                                //unimplemented MMIO address
                                throw new IOException($"MMIO address 0x{offset} is not connected to an input device (pc 0x{Convert.ToString(pc, 16)})");
                            }
                        }
                        else //reserved memory
                        {
                            throw new Exception($"Cannot load from address 0x{Convert.ToString(offset, 16)} in reserved memory (pc 0x{Convert.ToString(pc, 16)})");
                        }

                        //if necessary, mask loaded data
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //mask read data to signed byte
                                    if (showInstr || debug)
                                    {
                                        Console.Write("lb");
                                    }

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
                                    if (showInstr || debug)
                                    {
                                        Console.Write("lh");
                                    }

                                    loadVal = loadVal & HALF_MASK;
                                    if ((loadVal & HALF_SIGN_MASK) != 0)
                                    {
                                        loadVal = loadVal | H_SIGN_SET_MASK;
                                    }
                                    break;
                                }
                            case 2:
                                {
                                    if (showInstr || debug)
                                    {
                                        Console.Write("lw");
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    //mask read data to unsigned byte
                                    if (showInstr || debug)
                                    {
                                        Console.Write("lbu");
                                    }

                                    loadVal = loadVal & BYTE_MASK;
                                    break;
                                }
                            case 5:
                                {
                                    //mask read data to unsigned halfword
                                    if (showInstr || debug)
                                    {
                                        Console.Write("lhu");
                                    }

                                    loadVal = loadVal & HALF_MASK;
                                    break;
                                }
                            default:
                                {
                                    //unknown func3
                                    throw new Exception($"Instruction func3 does not correspond to any known instruction (pc 0x{Convert.ToString(pc, 16)})");
                                }
                        }
                        setRD(loadVal); //set value of rd to loaded value

                        if (showInstr || debug)
                        {
                            Console.WriteLine(" {0} 0x{1}({2})", REG_NAMES[GetRD()], Convert.ToString(GenerateImmed_I(), 16), REG_NAMES[GetRS1()]);
                        }
                        if (debug)
                        {
                            Console.WriteLine("loaded from address 0x{0}", Convert.ToString(offset, 16));
                            Console.WriteLine("rd {0} contains 0x{1}\n", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
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
                                    if (showInstr || debug)
                                    {
                                        Console.Write("addi");
                                    }
                                    setRD(regs[GetRS1()] + GenerateImmed_I());
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 by 5 LSB of i immed and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("slli");
                                    }
                                    setRD(regs[GetRS1()] << (Int32)(GenerateImmed_I() & SHIFT_MASK));
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than i immed (signed), 0 otherwise
                                    if (showInstr || debug)
                                    {
                                        Console.Write("slti");
                                    }
                                    setRD((Int32)regs[GetRS1()] < (Int32)GenerateImmed_I() ? 1u : 0u);
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than i immed (unsigned), 0 otherwise
                                    if (showInstr || debug)
                                    {
                                        Console.Write("sltiu");
                                    }
                                    setRD(regs[GetRS1()] < GenerateImmed_I() ? 1u : 0u);
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and i immed and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("xori");
                                    }
                                    setRD(regs[GetRS1()] ^ GenerateImmed_I());
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 by 5 LSB of i immed and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("srli");
                                        }
                                        setRD(regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK));
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 by 5 LSB of i immed and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("srai");
                                        }
                                        setRD((UInt32)((Int32)regs[GetRS1()] >> (Int32)(GenerateImmed_I() & SHIFT_MASK)));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and i immed and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("ori");
                                    }
                                    setRD(regs[GetRS1()] | GenerateImmed_I());
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and i immed and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("andi");
                                    }
                                    setRD(regs[GetRS1()] & GenerateImmed_I());
                                    break;
                                }
                        }

                        if (showInstr || debug)
                        {
                            Console.WriteLine(" {0} {1} 0x{2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], Convert.ToString(GenerateImmed_I(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                            Console.WriteLine("rd {0} contains 0x{1}\n", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                        }

                        break;
                    }
                case B_OPCODE:
                    {
                        switch ((ir & FUNC3_MASK) >> 12)
                        {
                            case 0:
                                {
                                    //add b immed to pc if values in rs1 and rs2 are equal
                                    if (showInstr || debug)
                                    {
                                        Console.Write("beq");
                                    }

                                    if (regs[GetRS1()] == regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //add b immed to pc if values in rs1 and rs2 are not equal
                                    if (showInstr || debug)
                                    {
                                        Console.Write("bne");
                                    }

                                    if (regs[GetRS1()] != regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 4:
                                {
                                    //add b immed to pc if value in rs1 is less than that in rs2
                                    if (showInstr || debug)
                                    {
                                        Console.Write("blt");
                                    }

                                    if ((Int32)regs[GetRS1()] < (Int32)regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 5:
                                {
                                    //add b immed to pc if value in rs1 is greater than or equal that in rs2
                                    if (showInstr || debug)
                                    {
                                        Console.Write("bge");
                                    }

                                    if ((Int32)regs[GetRS1()] >= (Int32)regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //add b immed to pc if value in rs1 is less than that in rs2 (unsigned)
                                    if (showInstr || debug)
                                    {
                                        Console.Write("bltu");
                                    }

                                    if (regs[GetRS1()] < regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            case 7:
                                {
                                    //add b immed to pc if value in rs1 is greater than or equal that in rs2 (unsigned)
                                    if (showInstr || debug)
                                    {
                                        Console.Write("bgeu");
                                    }

                                    if (regs[GetRS1()] >= regs[GetRS2()])
                                    {
                                        pc += GenerateImmed_B() - 4; //-4 to get pc before moving to next instruction
                                    }
                                    break;
                                }
                            default:
                                {
                                    //unknown func3
                                    throw new Exception($"Instruction func3 does not correspond to any known instruction (pc 0x{Convert.ToString(pc, 16)})");
                                }
                        }

                        if (showInstr || debug)
                        {
                            Console.WriteLine(" {0} {1} 0x{2}", REG_NAMES[GetRS1()], REG_NAMES[GetRS2()], Convert.ToString(GenerateImmed_B(), 16));
                        }
                        if (debug)
                        {
                            Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                            Console.WriteLine("rs2 {0} contains 0x{1}", REG_NAMES[GetRS2()], Convert.ToString(regs[GetRS2()], 16));
                            Console.WriteLine("pc is now 0x{0}\n", Convert.ToString(pc, 16));
                        }

                        break;
                    }
                case S_OPCODE:
                    {
                        UInt32 offset = GenerateImmed_S() + regs[GetRS1()];
                        UInt32 storeVal = regs[GetRS2()];

                        if (offset < DATA_ADDR) //storing to text segment - SHOULD NOT HAPPEN, but possible
                        {
                            text!.Seek(offset, SeekOrigin.Begin); //move text filestream to given offset

                            switch ((ir & FUNC3_MASK) >> 12)
                            {
                                case 0:
                                    {
                                        //store byte (8 bits) in text segment file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sb");
                                        }

                                        textWriter!.Write((byte)storeVal);
                                        break;
                                    }
                                case 1:
                                    {
                                        //store halfword (16 bits) in text segment file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sh");
                                        }

                                        textWriter!.Write((UInt16)storeVal);
                                        break;
                                    }
                                case 2:
                                    {
                                        //store word (32 bits) in text segment file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sw");
                                        }

                                        textWriter!.Write(storeVal);
                                        break;
                                    }
                                default:
                                    {
                                        //unknown func3
                                        throw new Exception("Instruction func3 does not correspond to any known instruction");
                                    }
                            }
                        }
                        else if (offset < STACK_ADDR) //storing to data segment
                        {
                            data!.Seek(offset - DATA_ADDR, SeekOrigin.Begin); //move data filestream to given offset
                            switch ((ir & FUNC3_MASK) >> 12)
                            {
                                case 0:
                                    {
                                        //store byte (8 bits) in data file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sb");
                                        }

                                        dataWriter!.Write((byte)storeVal);
                                        break;
                                    }
                                case 1:
                                    {
                                        //store halfword (16 bits) in data file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sh");
                                        }

                                        dataWriter!.Write((UInt16)storeVal);
                                        break;
                                    }
                                case 2:
                                    {
                                        //store word (32 bits) in data file
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sw");
                                        }

                                        dataWriter!.Write(storeVal);
                                        break;
                                    }
                                default:
                                    {
                                        //unknown func3
                                        throw new Exception("Instruction func3 does not correspond to any known instruction");
                                    }
                            }
                        }
                        else if (offset >= MMIO_ADDR) //storing to MMIO
                        {
                            switch ((ir & FUNC3_MASK) >> 12)
                            {
                                case 0:
                                    {
                                        //store byte (8 bits) to MMIO
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sb");
                                        }

                                        if (outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = (byte)storeVal;
                                            //Console.WriteLine($"{Convert.ToString(offset,16)}: {outputTable[offset]}");
                                        }
                                        else
                                        {
                                            //unimplemented MMIO address
                                            throw new IOException($"MMIO address 0x{Convert.ToString(offset,16)} is not connected to an output device (pc 0x{Convert.ToString(pc, 16)})");
                                        }
                                        break;
                                    }
                                case 1:
                                    {
                                        //store halfword (16 bits) to MMIO
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sh");
                                        }

                                        if (outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = (UInt16)storeVal;
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
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sw");
                                        }

                                        if (outputTable.ContainsKey(offset))
                                        {
                                            outputTable[offset] = storeVal;
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

                        if (showInstr || debug)
                        {
                            Console.WriteLine(" {0} 0x{1}({2})", REG_NAMES[GetRS2()], Convert.ToString(GenerateImmed_S(), 16), REG_NAMES[GetRS1()]);
                        }

                        //show MMIO output
                        if (offset >= MMIO_ADDR)
                        {
                            Console.WriteLine("0x{0}: {1}", Convert.ToString(offset, 16).PadLeft(8, '0'), Convert.ToString(outputTable[offset], 16).PadLeft(8, '0'));
                        }

                        if (debug)
                        {
                            Console.WriteLine("stored to address 0x{0}", Convert.ToString(offset, 16));
                            Console.WriteLine("rs2 {0} contains 0x{1}\n", REG_NAMES[GetRS2()], Convert.ToString(regs[GetRS2()], 16));
                        }

                        //change vga buffer if needed
                        if (offset == VGA_COLOR_ADDR)
                        {
                            try
                            {
                                vgaBuffer[outputTable[VGA_PIXEL_ADDR] / 128, outputTable[VGA_PIXEL_ADDR] % 128] = (byte)outputTable[VGA_COLOR_ADDR];
                            }
                            catch
                            {
                                throw new Exception($"Exception when writing to VGA on pc 0x{Convert.ToString(pc, 16)}");
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
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //add value in rs1 and value in rs2 and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("add");
                                        }

                                        setRD(regs[GetRS1()] + regs[GetRS2()]);
                                    }
                                    else
                                    {
                                        //subtract value in rs2 from value in rs1 and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sub");
                                        }

                                        setRD(regs[GetRS1()] - regs[GetRS2()]);
                                    }
                                    break;
                                }
                            case 1:
                                {
                                    //logical left shift value in rs1 and value in rs2 and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("sll");
                                    }

                                    setRD(regs[GetRS1()] << (Int32)(regs[GetRS2()] & SHIFT_MASK));
                                    break;
                                }
                            case 2:
                                {
                                    //write 1 to rd if value in rs1 (signed) is less than value in rs2 (signed), 0 otherwise
                                    if (showInstr || debug)
                                    {
                                        Console.Write("slt");
                                    }

                                    setRD((Int32)regs[GetRS1()] < (Int32)regs[GetRS2()] ? 1u : 0u);
                                    break;
                                }
                            case 3:
                                {
                                    //write 1 to rd if value in rs1 (unsigned) is less than value in rs2 (unsigned), 0 otherwise
                                    if (showInstr || debug)
                                    {
                                        Console.Write("sltu");
                                    }

                                    setRD(regs[GetRS1()] < regs[GetRS2()] ? 1u : 0u);
                                    break;
                                }
                            case 4:
                                {
                                    //xor value in rs1 and value in rs2 and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("xor");
                                    }

                                    setRD(regs[GetRS1()] ^ regs[GetRS2()]);
                                    break;
                                }
                            case 5:
                                {
                                    if ((ir & MSB7_MASK) == 0)
                                    {
                                        //logical right shift value in rs1 and value in rs2 and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("srl");
                                        }

                                        setRD(regs[GetRS1()] >> (Int32)(regs[GetRS2()] & SHIFT_MASK));
                                    }
                                    else
                                    {
                                        //arithmetic right shift value in rs1 and value in rs2 and write to rd
                                        if (showInstr || debug)
                                        {
                                            Console.Write("sra");
                                        }

                                        setRD((UInt32)((Int32)regs[GetRS1()] >> (Int32)(regs[GetRS2()] & SHIFT_MASK)));
                                    }
                                    break;
                                }
                            case 6:
                                {
                                    //or value in rs1 and value in rs2 and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("or");
                                    }

                                    setRD(regs[GetRS1()] | regs[GetRS2()]);
                                    break;
                                }
                            default: //7 - only other option
                                {
                                    //and value in rs1 and value in rs2 and write to rd
                                    if (showInstr || debug)
                                    {
                                        Console.Write("and");
                                    }

                                    setRD(regs[GetRS1()] & regs[GetRS2()]);
                                    break;
                                }
                        }
                        if (showInstr || debug)
                        {
                            Console.WriteLine(" {0} {1} {2}", REG_NAMES[GetRD()], REG_NAMES[GetRS1()], REG_NAMES[GetRS2()]);
                        }
                        if (debug)
                        {
                            Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                            Console.WriteLine("rs2 {0} contains 0x{1}", REG_NAMES[GetRS2()], Convert.ToString(regs[GetRS2()], 16));
                            Console.WriteLine("rd {0} contains 0x{1}\n", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
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
                                    mstatus = (mstatus & INTR_MASK) | ((mstatus & BYTE_SIGN_MASK) >> 4); //copy mpie bit to mie bit and clear mpie bit
                                    pc = mepc; //set pc to mepc (value before interrupt)

                                    if (showInstr || debug)
                                    {
                                        Console.WriteLine("mret");
                                    }
                                    if (debug)
                                    {
                                        Console.WriteLine("mstatus contains 0x{0}", Convert.ToString(mstatus, 16));
                                        Console.WriteLine("pc is now 0x{0}", Convert.ToString(pc, 16));
                                    }

                                    break;
                                }
                            case 1:
                                {
                                    //read csr value into rd and write value of rs1 into csr
                                    if (showInstr || debug)
                                    {
                                        Console.Write("csrrw");
                                    }

                                    UInt32 csr = GetCSR();
                                    if (csr == 0x341) //mepc
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
                                    if (showInstr || debug)
                                    {
                                        Console.Write("csrrs");
                                    }

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
                                    if (showInstr || debug)
                                    {
                                        Console.Write("csrrc");
                                    }

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
                        if ((ir & FUNC3_MASK) != 0)
                        {
                            if (showInstr || debug)
                            {
                                Console.WriteLine(" {0} 0x{1} {2}", REG_NAMES[GetRD()], Convert.ToString(GetCSR(), 16), REG_NAMES[GetRS1()]);
                            }
                            if (debug)
                            {
                                Console.WriteLine("rs1 {0} contains 0x{1}", REG_NAMES[GetRS1()], Convert.ToString(regs[GetRS1()], 16));
                                Console.WriteLine("rd {0} contains 0x{1}", REG_NAMES[GetRD()], Convert.ToString(regs[GetRD()], 16));
                                Console.WriteLine("mtvec contains 0x{0}", Convert.ToString(mtvec, 16));
                                Console.WriteLine("mepc contains 0x{0}", Convert.ToString(mepc, 16));
                                Console.WriteLine("mstatus contains 0x{0}", Convert.ToString(mstatus, 16));
                            }
                        }
                        break;
                    }
                default:
                    {
                        //unknown opcode
                        throw new Exception($"Instruction opcode in instruction 0x{Convert.ToString(ir, 16)} at pc 0x{Convert.ToString(pc,16)} does not correspond to any known instruction");
                    }
            }
        }

        //set rd to given value. ir must already contain current instruction
        private void setRD(UInt32 value)
        {
            UInt32 rd = GetRD();
            if (rd > 0)
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
            UInt32 imm = 0;
            for (int i = 0; i < 32; i++)
            {
                imm = imm | (i < 11 ? (ir & (1u << (i + 20))) >> 20 : (ir & (1u << 31)) >> (31 - i));
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
            imm = imm | (ir & (1 << 7)) << 4;
            for (int i = 12; i < 32; i++)
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
            for (int i = 1; i < 11; i++)
            {
                imm = imm | ((ir & (1u << (i + 20))) >> 20);
            }
            imm = imm | ((ir & (1 << 20)) >> 9);
            for (int i = 12; i < 32; i++)
            {
                imm = imm | (i < 20 ? (ir & (1u << i)) : (ir & (1u << 31)) >> (31 - i));
            }
            return imm;
        }
    }
}