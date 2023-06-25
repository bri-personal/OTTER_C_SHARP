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
                    Util.CopyBits(output, input);
                }
            }
        }
        
    }
    
    internal class CompMUX : Component
    {
        BitArray[] inputs;
        BitArray output;
        BitArray sel;

        public CompMUX(BitArray[] inputs, BitArray output, BitArray sel)
        {
            this.inputs = inputs;
            this.output = output;
            this.sel = sel;
        }

        public void Update()
        {
            Util.CopyBits(output, inputs[Util.BitArrayToInt32(sel)]);
        }
    }
    
}