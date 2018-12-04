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
        private List<string[]> processes;

        public PuppetMasterServices(string confFile)
        {
            Pcs = new List<PcsService>();
            string serverLoc;

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);


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

            processes = new List<string[]>();

            Console.WriteLine("Welcome Puppet Master");
        }

        public void StartServer(string serverId, string url, int minDelay, int maxDelay)
        {
            string result = null;
            string[] words = url.Split('/');
            words = words[2].Split(':');

            foreach (PcsService pc in Pcs)
            {                
                if(pc.Location.Equals(words[0]))
                {
                    AddProcess(serverId, words[0]);
                    result = pc.StartServer(serverId, url, minDelay, maxDelay);                    
                    break;
                }                
            }
            if (result == null)
                Console.WriteLine("Invalid Location");
            else
                Console.WriteLine(result);
        }

        public void StartClient(string serverId, string url, string scriptFile)
        {
            string result = null;
            string[] words = url.Split('/');
            words = words[2].Split(':');

            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals(words[0]))
                {
                    AddProcess(serverId, words[0]);
                    result = pc.StartClient(serverId, url, scriptFile);                    
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

        public void Crash(string serverId)
        {
            string result = null;

            string url = GetLocation(serverId);

            foreach (PcsService pc in Pcs)
            {
                if (pc.Location.Equals(url))
                {
                    result = pc.Crash(serverId);
                    RemoveProcess(serverId);
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

        private void AddProcess(string serverId, string url)
        {
            string[] aux = new string[2];
            aux[0] = serverId;
            aux[1] = url;
            processes.Add(aux);
        }

        private void RemoveProcess(string serverId)
        {
            foreach(string[] ps in processes)
            {
                if(ps[0].Equals(serverId))
                {
                    processes.Remove(ps);
                    break;
                }
            }
        }

        private string GetLocation(string serverId)
        {            
            foreach (string[] s in processes)
            {
                if (s[0].Equals(serverId))
                {
                    return s[1];
                }
            }
            return null;
        }
    }
}
