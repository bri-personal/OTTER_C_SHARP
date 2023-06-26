using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

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

    internal class CompMemory : Component
    {
        BitArray rdEn1, rdEn2;
        BitArray we2;
        BitArray clock;
        BitArray addr1, addr2, dIn2;
        BitArray ioIn;
        BitArray ir, dOut2; //ir also used for size and sign
        BitArray ioWr;

        BitArray mem;

        public CompMemory(BitArray rdEn1, BitArray rdEn2, BitArray we2, BitArray clock, BitArray addr1, BitArray addr2, BitArray dIn2, BitArray ir, BitArray ioIn, BitArray dOut2, BitArray ioWr)
        {
            this.rdEn1 = rdEn1;
            this.rdEn2 = rdEn2;
            this.we2 = we2;
            this.clock = clock;
            this.addr1 = addr1;
            this.addr2 = addr2;
            this.dIn2 = dIn2;
            this.ir = ir;
            this.ioIn = ioIn;
            this.dOut2 = dOut2;
            this.ioWr = ioWr;

            mem = new BitArray(1024);
        }

        public BitArray Mem { get => mem; }

        public void Update()
        {
            if (clock[0])
            {
                if (rdEn1[0])
                {
                    //copy 32 bits from instruction (from text segment) to ir
                    Console.WriteLine("dOut1 before:\n{0}", Util.BitsToString(ir));
                    Util.CopyBits(ir, mem, Util.BitArrayToInt32(addr1) * 8);
                    Console.WriteLine("dOut1 copied:\n{0}", Util.BitsToString(ir));
                }

                //get size for read/write from data segment
                int size;
                if (!ir[12] && !ir[13]) //size=0 -> byte
                {
                    size = 8;
                }
                else if (ir[12] && !ir[13]) //size=1 -> halfword
                {
                    size = 16;
                }
                else //size=2 or 3 (default) -> word
                {
                    size = 32;
                }
                Console.WriteLine("Size to read/write: {0}",size);

                if (rdEn2[0]) //load
                {
                    Console.WriteLine("Addr2: {0}",Util.BitArrayToInt32(addr2));

                    //copy size # of bits from mem to dOut2 and extend to 32 bits
                    Console.WriteLine("dOut2 before:\n{0}", Util.BitsToString(dOut2));
                    Util.CopyBits(dOut2, mem, Util.BitArrayToInt32(addr2) * 8, size);
                    Console.WriteLine("dOut2 copied:\n{0}", Util.BitsToString(dOut2));
                    if (ir[14]) //0 extend
                    {
                        for(int i= Util.BitArrayToInt32(addr2) * 8+size; i < dOut2.Length; i++)
                        {
                            dOut2[i] = false;
                        }
                    }
                    else //sign extend
                    {
                        for (int i = Util.BitArrayToInt32(addr2) * 8+size; i < dOut2.Length; i++)
                        {
                            dOut2[i] = dOut2[size-1];
                        }
                    }
                    Console.WriteLine("dOut2 extended:\n{0}", Util.BitsToString(dOut2));
                }
                if (we2[0]) //store
                {
                    //copy size # of bits from dIn2 to mem
                    Util.CopyBits(mem, dIn2, len: size, destStart: Util.BitArrayToInt32(addr2) * 8);
                }
            }
        }
    }
    
    
    internal class CompRegFile : Component
    {
        BitArray ir; //includes adr1, adr2, wa
        BitArray clock;
        BitArray wd;
        BitArray en;
        BitArray rs1, rs2;

        private BitArray[] regs;

        public CompRegFile(BitArray ir, BitArray clock, BitArray wd, BitArray en, BitArray rs1, BitArray rs2)
        {
            this.ir = ir;
            this.clock = clock;
            this.wd = wd;
            this.en = en;
            this.rs1 = rs1;
            this.rs2 = rs2;

            //initialize registers
            regs = new BitArray[32];
            for(int i= 0; i < regs.Length; i++)
            {
                regs[i]=new BitArray(32);
            }
        }

        public BitArray[] Regs { get => regs; }

        public void Update()
        {
            //asynch read
            Util.CopyBits(rs1, regs[Util.BitArrayToInt32(ir, 15, 5)]);
            Util.CopyBits(rs2, regs[Util.BitArrayToInt32(ir, 20, 5)]);

            //synch write
            if (clock[0])
            {
                if (en[0])
                {
                    Util.CopyBits(regs[Util.BitArrayToInt32(ir, 7, 5)], wd);
                }
            }
        }
    }
}