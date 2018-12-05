using ClientLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PcsService : MarshalByRefObject
    {
        private List<Object[]> processes;

        private string location;
        private string type;
        private string serverLoc;

        public string Location { get { return this.location; } }

        public PcsService(string location, string type, string serverLoc)
        {
            this.location = location;
            processes = new List<Object[]>();
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
                Arguments = port + " " + type + " null " + minDelay + " " + maxDelay + " " + objName + " " + serverID
            };
            try
            {
                if (StartProcess(serverID, startInfo, url))
                    return "Id already in use";
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
                Arguments = serverLoc + " " + type + " " + scriptFile + " " + serverID
            };

            try
            {
                if(StartProcess(serverID, startInfo, url))
                    return "Id already in use";
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
            foreach(Object[] obj in processes)
            {
                try
                {
                    if (obj[2] != null)
                        ((IServerService)obj[2]).Status();
                }
                catch
                {
                    processes.Remove(obj);
                }
            }
        } 

        public string Crash(string url)
        {
            string result = "Process Closed";
            foreach(Object[] loc in processes)
            {
                if (url.Equals(loc[0]))
                {
                    try
                    {
                        Process.GetProcessById(System.Convert.ToInt32(loc[1])).Kill();                        
                    }
                    catch(ArgumentException)
                    {
                        Console.WriteLine("Process already closed");
                        result = "Process already closed";
                    }
                    finally
                    {
                        processes.Remove(loc);                        
                    }
                    break;
                }
            }
            return result;
        }

        public string Freeze(string serverId)
        {
            string result = "Process Frozzen";
            foreach (Object[] loc in processes)
            {
                if (serverId.Equals(loc[0]))
                {
                    try
                    {
                        ((IServerService)loc[2]).Freeze(true);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Process already closed");
                        result = "Process already closed";
                        processes.Remove(loc);
                    }
                    break;
                }
            }
            return result;
        }

        public string  Unfreeze(string serverId)
        {
            string result = "Process Unfrozzen";
            foreach (Object[] loc in processes)
            {
                if (serverId.Equals(loc[0]))
                {
                    try
                    {
                        ((IServerService)loc[2]).Freeze(false);
                    }
                    catch (ArgumentException)
                    {
                        Console.WriteLine("Process already closed");
                        result = "Process already closed";
                        processes.Remove(loc);
                    }
                    break;
                }
            }
            return result;
        }

        private bool StartProcess(string serverId, ProcessStartInfo startInfo, string url)
        {
            if (CheckServerId(serverId))
            {
                Console.WriteLine("Id already in use");
                return true;
            }
            using (Process exeProcess = Process.Start(startInfo))
            {
                Console.WriteLine("{0} Comecou", serverId);
                Console.WriteLine(exeProcess.Id);
                IServerService obj;
                
                try
                {
                    obj = (IServerService)Activator.GetObject(
                        typeof(IServerService),
                        url);
                }
                catch
                {
                    obj = null;
                }
                //obj.Status();
                Object[] aux = new Object[3] { serverId, System.Convert.ToString(exeProcess.Id), obj };
                processes.Add(aux);
                             
                exeProcess.WaitForExit();
                Console.WriteLine("{0} Terminou ", serverId);
            }
            return false;
        }

        private bool CheckServerId(string serverId)
        {
            foreach(Object[] obj in processes)
            {
                if (obj[0].Equals(serverId))
                    return true;
            }
            return false;
        }
    }
}
