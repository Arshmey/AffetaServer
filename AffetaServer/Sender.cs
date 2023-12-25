using AffetaServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace AffetaServer
{
    internal class Sender
    {
        Server server;
        Dictionary<string, TcpClient> client;
        Dictionary<string, NetworkStream> stream;
        Dictionary<string, Wallet> wallet;
        Block block;
        Thread senderMSG;
        private string ID;
        private string msg;
        byte[] bufSend = new byte[8192];

        public Sender(Server server, Dictionary<string, NetworkStream> stream, Dictionary<string, TcpClient> client, Dictionary<string, Wallet> wallet, Block block, string ID)
        {
            this.server = server;
            this.stream = stream;
            this.client = client;
            this.wallet = wallet;
            this.block = block;
            this.ID = ID;

            senderMSG = new Thread(SenderMSG);
            senderMSG.Start();
        }

        private void SenderMSG()
        {
            while (true)
            {
                try
                {
                    byte[] bufRead = new byte[client[ID].ReceiveBufferSize];
                    stream[ID].Read(bufRead, 0, bufRead.Length);
                    bufRead = bufRead.Where(x => x > 0).ToArray();
                    msg = Encoding.UTF8.GetString(bufRead);
                }
                catch (Exception ex)
                {
                    StopAll();
                    break;
                }

                try
                {
                    if (stream[ID].CanWrite)
                    {
                        switch (msg)
                        {
                            case "Mining":
                                block.setBlock(block.getBlock() - 1);
                                wallet[ID].setCoin();
                                File.WriteAllText(@"Afeta_Data/" + ID + "/Wallet.txt", wallet[ID].getCoin().ToString("0.#######"));
                                break;
                            case "Balance":
                                bufSend = Encoding.UTF8.GetBytes("Balance<:>" + wallet[ID].getCoin().ToString("0.#######"));
                                stream[ID].Write(bufSend, 0, bufSend.Length);
                                stream[ID].Flush();
                                break;
                            default:
                                string[] split = msg.Split(new string[] { "<:>" }, StringSplitOptions.None);
                                wallet[ID].setCoin(wallet[ID].getCoin() - double.Parse(split[1]));
                                wallet[split[0]].setCoin(wallet[split[0]].getCoin() + double.Parse(split[1]));

                                bufSend = Encoding.UTF8.GetBytes("Transfer<:>" + wallet[ID].getCoin().ToString("0.#######"));
                                stream[split[0]].Write(bufSend, 0, bufSend.Length);
                                stream[split[0]].Flush();
                                File.WriteAllText(@"Afeta_Data/" + ID + "/Wallet.txt", wallet[ID].getCoin().ToString("0.#######"));
                                File.WriteAllText(@"Afeta_Data/" + split[0] + "/Wallet.txt", wallet[ID].getCoin().ToString("0.#######"));
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void StopAll()
        {
            stream[ID].Close();
            server.deleteVoidClient(ID);
            senderMSG.Abort();
        }

        public string MSGlog()
        {
            return msg;
        }

    }
}
