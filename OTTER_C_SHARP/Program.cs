using System.Collections;

namespace Otter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MCU otter = new MCU(); //create OTTER object
        }
    }
    
    public unsafe class MCU
    {
        //components
        CompPC pc;
        //CompMUX pcMux;

        /*signals*/
        BitArray clock;

        //PC
        BitArray reset, pcWrite, pcIn, pcOut;

        //PC MUX
        BitArray pc4, jalr, branch, jal, mtvec, mepc, pcSource;

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


            /*initialize components*/
            CompPC pc = new CompPC(reset, pcWrite, pcIn, pcOut, clock);
            CompMUX pcMux = new CompMUX(new BitArray[6] {pc4, jalr, branch, jal, mtvec, mepc}, pcIn, pcSource);

            clock[0] = true;

            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));
            pcSource[0] = true; //pcSource=1
            jalr[0] = true; //jalr=1
            pcMux.Update(); //mux out=1
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));

            pcWrite[0] = true; //write=on
            pc.Update(); //pc out = jalr = 1
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));
            reset[0] = true; //reset  = 1
            pc.Update(); //pc out = 0
            Console.WriteLine("PC IN: {0}\nPC OUT: {1}\n", Util.BitArrayToInt32(pcIn), Util.BitArrayToInt32(pcOut));
        }
    }
}