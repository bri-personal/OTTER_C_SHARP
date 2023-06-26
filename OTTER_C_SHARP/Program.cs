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
        Component[] components;

        /*signals*/
        BitArray clock;

        //PC
        BitArray reset, pcWrite, pcIn, pcOut;

        //PC MUX
        BitArray pc4, jalr, branch, jal, mtvec, mepc, pcSource;

        //Memory
        BitArray memRdEn1, memRdEn2, memWE2, ir, result, rs2Out, memOut2, iobusIn, ioWr;

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
            rs2Out = new BitArray(32);
            memOut2 = new BitArray(32);
            iobusIn = new BitArray(32);
            ioWr = new BitArray(1);


            /*initialize components*/
            pc = new CompPC(reset, pcWrite, pcIn, pcOut, clock);
            pcMux = new CompMUX(new BitArray[6] {pc4, jalr, branch, jal, mtvec, mepc}, pcIn, pcSource);
            memory = new CompMemory(memRdEn1, memRdEn2, memWE2, clock, pcOut, result, rs2Out, ir, iobusIn, ir, memOut2, ioWr);
            components = new Component[] { pcMux, pc, memory }; //array to iterate through to update

            clock[0] = true;
            
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
            ir[12] = true; //size =16
            ir[14] = true; //unsigned
            UpdateAll(); //memory is 8 '0's followed by 8 '1's
            PrintMem();

            memWE2[0] = false; //memory write=off
            memRdEn2[0] = true; //memory read2 = on
            UpdateAll(); //memout2 is updated
            Console.WriteLine("Memory Out 2: {0}", Util.BitArrayToInt32(memOut2));

            ir[12] = false;
            ir[14] = false;
            memRdEn1[0] = true; //memory read1 = on
            memRdEn2[0] = false; //memory read2 = off
            UpdateAll(); //ir is updated
            Console.WriteLine("Memory Out 1: {0}", Util.BitArrayToInt32(ir));
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