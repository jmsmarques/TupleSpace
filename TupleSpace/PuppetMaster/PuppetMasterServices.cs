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

        public void StartServer(string serverID, string url, int minDelay, int maxDelay)
        {
            string result = null;
            string[] words = url.Split('/');
            words = words[2].Split(':');

            foreach (PcsService pc in Pcs)
            {                
                if(pc.Location.Equals(words[0]))
                {
                    result = pc.StartServer(serverID, url, minDelay, maxDelay);
                    break;
                }                
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
        }

        public void StartClient(string serverID, string url, string scriptFile)
        {
            string result = null;    
            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals("localhost"))
                {
                    result = pc.StartClient(serverID, url, scriptFile);
                    break;
                }                
            }

            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);      
        }

        public void PrintStatus()
        {
            foreach (PcsService pc in Pcs)
            {
                pc.PrintStatus();
            }
        }

        public void Crash(string url, string serverId)
        {
            string result = null;
            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals("localhost:" + url))
                {
                    result = pc.Crash(serverId);
                    break;
                }
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
        }

        public void Freeze(string url, string serverId)
        {
            string result = null;
            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals("localhost:" + url))
                {
                    result = pc.Crash(serverId);
                    break;
                }
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
        }

        public void Unfreeze(string url, string serverId)
        {
            string result = null;
            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals("localhost:" + url))
                {
                    result = pc.Crash(serverId);
                    break;
                }
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
        }
    }
}
