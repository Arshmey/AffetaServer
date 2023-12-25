using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AffetaServer
{
    internal class Server
    {

        TcpListener listener = new TcpListener(IPAddress.Any, 7777);
        Dictionary<string, TcpClient> client = new Dictionary<string, TcpClient>();
        Dictionary<string, NetworkStream> stream = new Dictionary<string, NetworkStream>();
        Dictionary<string, Sender> senderThread = new Dictionary<string, Sender>();
        Dictionary<string, Wallet> wallet = new Dictionary<string, Wallet>();
        Block block;
        string msg;
        string id = "";
        bool isEnd = true;

        public Server() 
        {
            Console.WriteLine("Write you have give to day blocks.");
            block = new Block(int.Parse(Console.ReadLine()));
            listener.Start();
            Console.Clear();
            new Thread(Blocks).Start();
            Thread acceptThread = new Thread(Start);
            acceptThread.Start();
        }

        private void Start()
        {
            while (true)
            {
                TcpClient clientWait = listener.AcceptTcpClient();
                NetworkStream streamWait = clientWait.GetStream();
                UserConnected(clientWait, streamWait);
                while (isEnd) { Console.WriteLine("Waiting"); }
                if (id != "null")
                {
                    client.Add(id, clientWait);
                    stream.Add(id, clientWait.GetStream());
                    wallet.Add(id, new Wallet(id));
                    senderThread.Add(id, new Sender(this, stream, client, wallet, block, id));
                }
                else { streamWait.Close(); clientWait.Close(); }
                isEnd = true;
            }
        }

        private void Blocks()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("Block haved: " + block.getBlock());
                if (block.getBlock() < 5)
                {
                    Environment.Exit(0);
                }
                Thread.Sleep(100);
            }
        }

        private void UserConnected(TcpClient clientWait, NetworkStream streamWait)
        {
            byte[] bufRead = new byte[clientWait.ReceiveBufferSize];
            streamWait.Read(bufRead, 0, bufRead.Length);
            bufRead = bufRead.Where(x => x > 0).ToArray();
            msg = Encoding.UTF8.GetString(bufRead);
            string[] processed = msg.Split(new string[] { "<:>" }, StringSplitOptions.None);

            switch (processed[0])
            {
                case "Login":
                    if (Directory.Exists(@"Afeta_Data/" + processed[1]) && File.ReadAllText(@"Afeta_Data/" + processed[1] + "/Pass.txt") == processed[2])
                    {
                        id = processed[1];
                        isEnd = false;
                    }
                    else
                    {
                        id = "null";
                        isEnd = false;
                    }
                    break;
                case "Reg":
                    if (!Directory.Exists(@"Afeta_Data/ " + processed[1]))
                    {
                        Directory.CreateDirectory(@"Afeta_Data/" + processed[1]);
                        File.Create(@"Afeta_Data/" + processed[1] + "/Wallet.txt").Close();
                        File.WriteAllText(@"Afeta_Data/" + processed[1] + "/Wallet.txt", "0");
                        File.Create(@"Afeta_Data/" + processed[1] + "/Pass.txt").Close();
                        File.WriteAllText(@"Afeta_Data/" + processed[1] + "/Pass.txt", processed[2]);
                        id = processed[1];
                        isEnd = false;
                    }
                    break;
                default:
                    isEnd = false;
                    break;
            }
        }

        public void deleteVoidClient(string ID)
        {
            client[ID].Close();
            client.Remove(ID);
            stream.Remove(ID);
            wallet.Remove(ID);
            senderThread.Remove(ID);
        }

    }
}
