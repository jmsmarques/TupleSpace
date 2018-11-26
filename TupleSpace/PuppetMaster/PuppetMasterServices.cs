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
            string result = null;
            foreach(PcsService pc in Pcs)
            {
                if(pc.Location.Equals("localhost:" + Url))
                {
                    result = pc.StartServer(serverID, serverID, 1, 1);
                    break;
                }                
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
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
