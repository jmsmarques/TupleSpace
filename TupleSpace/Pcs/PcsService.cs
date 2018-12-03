using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PcsService : MarshalByRefObject
    {
        private List<string[]> processes;

        private string location;
        private string type;
        private string serverLoc;

        public string Location { get { return this.location; } }

        public PcsService(string location, string type, string serverLoc)
        {
            this.location = location;
            processes = new List<string[]>();
            this.type = type;
            this.serverLoc = serverLoc;
        }

        public string StartServer(string serverID, string url, int minDelay, int maxDelay)
        {
            string port, objName;

            string[] words = url.Split('/');
            string[] words1 = words[2].Split(':');

            objName = words[3];
            port = words1[1];

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                FileName = "..\\..\\..\\Server\\bin\\Debug\\Server.exe",
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = port + " " + type + " null " + minDelay + " " + maxDelay + " " + objName
            };
            try
            {
                StartProcess(serverID, startInfo);
            }
            catch
            {
                Console.WriteLine("Exe doesn't exist");
                return "Unable to start server";
                // Log error.
            }
            return "Server finished at " + url;
        }

        public string StartClient(string serverID, string url, string scriptFile)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                FileName = "..\\..\\..\\Client\\bin\\Debug\\Client.exe",
                WindowStyle = ProcessWindowStyle.Normal,
                Arguments = serverLoc + " " + type + " " + scriptFile
            };

            try
            {
                StartProcess(serverID, startInfo);
            }
            catch
            {
                Console.WriteLine("Exe doesn't exist");
                return "Unable to start client";
                // Log error.
            }
            return "Client finished at " + url;
        }

        public void PrintStatus()
        {
            
        } 

        public string Crash(string url)
        {
            foreach(string[] loc in processes)
            {
                if (url.Equals(loc[0]))
                {
                    Process.GetProcessById(System.Convert.ToInt32(loc[1])).Kill();
                    processes.Remove(loc);
                    break;
                }
            }
            return "Process Closed";
        }

        public string Freeze(string url)
        {
            foreach (string[] loc in processes)
            {
                if (url.Equals(loc[0]))
                {
                    //Process.GetProcessById(System.Convert.ToInt32(loc[1])).Kill();
                    break;
                }
            }
            return "Process Frezeed";
        }

        public string  Unfreeze(string url)
        {
            foreach (string[] loc in processes)
            {
                if (url.Equals(loc[0]))
                {
                    //Process.GetProcessById(System.Convert.ToInt32(loc[1])).Kill();
                    break;
                }
            }
            return "Process Resumed";
        }

        private void StartProcess(string serverID, ProcessStartInfo startInfo)
        {
            using (Process exeProcess = Process.Start(startInfo))
            {
                Console.WriteLine("{0} Comecou", serverID);
                Console.WriteLine(exeProcess.Id);
                string[] aux = new string[2];
                aux[0] = serverID;
                aux[1] = System.Convert.ToString(exeProcess.Id);
                processes.Add(aux);
                exeProcess.WaitForExit();
                Console.WriteLine("{0} Terminou ", serverID);
            }
        }
    }
}
