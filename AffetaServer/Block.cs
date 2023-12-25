using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffetaServer
{
    internal class Block
    {
        private int block = 0;

        public Block(int block)
        {
            this.block = block;
        }

        public void setBlock(int block)
        {
            this.block = block;
        }

        public int getBlock() 
        { 
            return block;
        }
    }
}
