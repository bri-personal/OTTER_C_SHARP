using System.Collections;

namespace Otter
{
    public class Program
    {
        public static void Main(string[] args)
        {
            MCU otter = new MCU();
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
            clock[0] = true;
            pcIn[0] = true;
            pcWrite[0] = true;
            Console.WriteLine(pcOut[0]);
            pc.Update();
            Console.WriteLine(pcOut[0]);
            reset[0] = true;
            pc.Update();
            Console.WriteLine(pcOut[0]);
        }
    }
}