using System.Collections;

namespace Otter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MCU otter = new MCU(); //create OTTER object

            /*
            BitArray b = new BitArray(5);
            BitArray c= new BitArray(5, true);
            Console.WriteLine("{0} {1}",Util.BitArrayToInt32(b), Util.BitArrayToInt32(c));
            Util.CopyBits(b, c, 1, 3);
            Console.WriteLine("{0} {1}", Util.BitArrayToInt32(b), Util.BitArrayToInt32(c));
            Util.CopyBits(c, b, destStart: 4);
            Console.WriteLine("{0} {1}", Util.BitArrayToInt32(b), Util.BitArrayToInt32(c));
            */
        }
    }
    
    public class MCU
    {
        //components
        CompPC pc;
        CompMUX pcMux;
        CompMemory memory;
        CompRegFile regFile;
        Component[] components;

        /*signals*/
        BitArray clock;

        //PC
        BitArray reset, pcWrite, pcIn, pcOut;

        //PC MUX
        BitArray pc4, jalr, branch, jal, mtvec, mepc, pcSource;

        //Memory
        BitArray memRdEn1, memRdEn2, memWE2, ir, result, memOut2, iobusIn, ioWr;

        //Reg File
        BitArray regWrite, regWD, rs1Out, rs2Out;

        public MCU()
        {
            /*initialize signals*/
            clock = new BitArray(1);

            //PC
            reset = new BitArray(1);
            pcWrite = new BitArray(1);
            pcIn = new BitArray(32);
            pcOut = new BitArray(32);

            //PC MUX
            pc4 = new BitArray(32);
            jalr = new BitArray(32);
            branch = new BitArray(32);
            jal = new BitArray(32);
            mtvec = new BitArray(32);
            mepc = new BitArray(32);
            pcSource = new BitArray(3);

            //Memory
            memRdEn1 = new BitArray(1);
            memRdEn2 = new BitArray(1);
            memWE2 = new BitArray(1);
            ir = new BitArray(32);
            result = new BitArray(32);
            memOut2 = new BitArray(32);
            iobusIn = new BitArray(32);
            ioWr = new BitArray(1);

            //Reg File
            regWrite=new BitArray(1);
            regWD = new BitArray(32);
            rs1Out = new BitArray(32);
            rs2Out = new BitArray(32);


            /*initialize components*/
            pc = new CompPC(reset, pcWrite, pcIn, pcOut, clock);
            pcMux = new CompMUX(new BitArray[6] {pc4, jalr, branch, jal, mtvec, mepc}, pcIn, pcSource);
            memory = new CompMemory(memRdEn1, memRdEn2, memWE2, clock, pcOut, result, rs2Out, ir, iobusIn, memOut2, ioWr);
            regFile = new CompRegFile(ir, clock, regWD, regWrite, rs1Out, rs2Out);
            components = new Component[] { pcMux, pc, memory, regFile }; //array to iterate through to update

            clock[0] = true;

            /*
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));
            pcSource[0] = true; //pcSource=1
            jalr[0] = true; //jalr=1
            UpdateAll(); //mux out=1
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));

            pcWrite[0] = true; //pc write=on
            UpdateAll(); //pc out = jalr = 1
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));
            PrintMem();

            memWE2[0]= true; //memory write=on
            rs2Out.SetAll(true); //memory din2 set to 1s
            result[0]=true; //memory addr2 = 1
            ir[14] = true; //unsigned (size=8)
            UpdateAll(); //memory is 8 '0's followed by 8 '1's
            PrintMem();

            memWE2[0] = false; //memory write=off
            memRdEn2[0] = true; //memory read2 = on
            UpdateAll(); //memout2 is updated
            Console.WriteLine("Memory Out 2: {0}", Util.BitArrayToInt32(memOut2));

            ir[12] = false; //reset size to not interfere
            ir[14] = false; //reset sign to not interfere
            memRdEn1[0] = true; //memory read1 = on
            memRdEn2[0] = false; //memory read2 = off
            UpdateAll(); //ir is updated
            Console.WriteLine("Memory Out 1: {0}", Util.BitArrayToInt32(ir));
            */

            Console.WriteLine("RegFile reg 1 before:\n{0}", Util.BitsToString(regFile.Regs[1]));
            regWD[0] = true; //write data =1
            ir[7] = true; //wa =1
            regWrite[0] = true; //regfile write enable= on
            UpdateAll(); //1 written to register 1
            Console.WriteLine("RegFile reg 1 written:\n{0}", Util.BitsToString(regFile.Regs[1]));

            regWrite[0] = false; //regfile write = off
            ir[15] = true; //adr1 = 1
            ir[20] = true; //adr2 =1
            UpdateAll(); //adr1 and adr2 show 1
            Console.WriteLine("RS1:\n{0}", Util.BitsToString(rs1Out));
            Console.WriteLine("RS2:\n{0}", Util.BitsToString(rs2Out));
        }

        public void UpdateAll()
        {
            foreach(Component c in components)
            {
                c.Update();
            }
        }

        public void PrintMem()
        {
            Console.WriteLine("Memory:");
            for (int i = 0; i < 25; i++)
            {
                Console.Write("{0} ", memory.Mem[i] ? 1 : 0);
            }
            Console.WriteLine();
        }
    }
}