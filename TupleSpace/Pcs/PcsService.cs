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
        public void StartServer(string serverIDd, string Url, int minDelay, int maxDelay)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = true;
            startInfo.FileName = "Server.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Normal;
            startInfo.Arguments = Url + " SMR " + "null";
            try
            {
                using (Process exeProcess = Process.Start(startInfo))
                {
                    Console.WriteLine("Comecou");
                    exeProcess.WaitForExit();
                    Console.WriteLine("Terminou");
                }
            }
            catch
            {
                Console.WriteLine("Exe doesn't exist");
                // Log error.
            }
            //return null;
        }

        public void StartClient(string serverIDd, string Url, string scriptFile)
        {
            // Use ProcessStartInfo class
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "Client.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.Arguments = "";

            try
            {
                // Start the process with the info we specified.
                // Call WaitForExit and then the using statement will close.
                using (Process exeProcess = Process.Start(startInfo))
                {
                    exeProcess.WaitForExit();
                }
            }
            catch
            {
                // Log error.
            }
        }

        public void PrintStatus()
        {

        }

    }
}
