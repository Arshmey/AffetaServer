using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AffetaServer
{
    internal class Wallet
    {

        private string name = "";
        private double coin = 0;

        public Wallet(string name)
        {
            this.name = name;
            Load();
        }

        private void Load()
        {
            coin = double.Parse(File.ReadAllText(@"Afeta_Data/" + name + "/Wallet.txt"));
        }

        public void setCoin()
        {
            coin += 0.0000001f;
        }

        public void setCoin(double coin)
        {
            this.coin = coin;
        }

        public double getCoin() 
        {
            return coin;
        }

    }
}
