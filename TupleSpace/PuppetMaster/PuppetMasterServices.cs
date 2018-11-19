using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class PuppetMasterServices
    {
        private List<PcsService> Pcs;

        public PuppetMasterServices(string confFile)
        {
            Pcs = new List<PcsService>();
            string serverLoc;

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, false);


            using (StreamReader file = File.OpenText(confFile))
            {
                string line;       
                while ((line = file.ReadLine()) != null)
                {
                    serverLoc = "tcp://" + line + ":" + "10000/PcsService";
                    
                    PcsService obj = (PcsService)Activator.GetObject(
                        typeof(PcsService),
                        serverLoc);

                    Pcs.Add(obj);
                }                  
            }

            Console.WriteLine("Welcome Puppet Master");
        }

        public void StartServer(string serverID, string Url, int minDelay, int maxDelay)
        {
            foreach(PcsService pc in Pcs)
            {
                pc.StartServer(serverID, Url, 1, 1);
            }
            Console.WriteLine("Server Started at {0}", Url);
        }

        public void StartClient(string serverID, string Url, string scriptFile)
        {
            foreach (PcsService pc in Pcs)
            {
                pc.StartClient(serverID, Url, scriptFile);
            }
        }

        public void PrintStatus()
        {
            foreach (PcsService pc in Pcs)
            {
                pc.PrintStatus();
            }
        }
    }
}
