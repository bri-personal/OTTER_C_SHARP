using System.Collections;

namespace Otter
{
    internal interface Component
    {
        void Update();
    }
    internal class CompPC : Component
    {
        private BitArray reset;
        private BitArray pcWrite;
        private BitArray clock;
        private BitArray input;
        private BitArray output;

        public CompPC(BitArray reset, BitArray pcWrite, BitArray input, BitArray output, BitArray clock)
        {
            this.reset = reset;
            this.pcWrite = pcWrite;
            this.clock = clock;
            this.input = input;
            this.output = output;
        }

        public void Update()
        {
            if (clock[0])
            {
                if (reset[0])
                {
                    output.SetAll(false);
                }
                else if (pcWrite[0])
                {
                    for(int i = 0; i<output.Length; i++)
                    {
                        output.Set(i, input[i]);
                    }
                }
            }
        }
        
    }
    /*
    internal class CompMUX : Component
    {
        UInt32 * [] inputs;
        UInt32 * output;
        UInt32 * sel;

        public CompMUX(UInt32* []inputs, UInt32 *output, UInt32 *sel)
        {
            this.inputs = inputs;
            this.output = output;
            this.sel = sel;
            Console.WriteLine((int)(&inputs));
        }

        public void Update()
        {
            *output = *inputs[*sel];
        }
    }
    */
}