using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Diagnostics;
using System.IO;

namespace PuppetMaster
{
    class PuppetMaster
    {
        static void Main(string[] args)
        {
            //port 10001 reserved
            Console.WriteLine("Configuration File");

            string file = Console.ReadLine();

            PuppetMasterServices puppetMaster = new PuppetMasterServices(file);

            Exec(puppetMaster);
        }

        static void Exec(PuppetMasterServices pcs)
        {
            string line;
            bool go = true;
            Console.WriteLine("Welcome Puppet Master");
            while(go)
            {
                try
                {
                    //System.IO.StreamReader file = new System.IO.StreamReader(input);
                    while ((line = Console.ReadLine()) != null)
                    {
                        //System.Console.WriteLine(line);
                        string[] words = line.Split(' ');
                        switch (words[0])
                        {
                            case "Server":
                                Console.WriteLine("...");
                                pcs.StartServer(words[1], words[2], 1, 1);
                                break;
                            case "Client":
                                pcs.StartClient(words[1], words[2], words[3]);
                                break;
                            case "Status":
                                pcs.PrintStatus();
                                break;
                            case "exit":
                            case "Exit":
                                go = false;
                                break;
                            default:
                                Console.WriteLine("Invalid command");
                                break;
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("File doesn't exists");
                }
            }
        }

        
    }
}
