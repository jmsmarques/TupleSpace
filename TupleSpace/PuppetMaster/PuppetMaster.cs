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
            string file;

            if (args.Length == 0)
            {
                Console.WriteLine("Configuration File");
                file = Console.ReadLine();
            }
            else
            {
                file = args[0];
            }
            
            PuppetMasterServices puppetMaster = new PuppetMasterServices(file);

            Exec(puppetMaster);
        }

        static void Exec(PuppetMasterServices pcs)
        {
            string line;
            bool go = true;
            
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
                                Task.Run(() => pcs.StartServer(words[1], words[2], 1, 1));
                                break;
                            case "Client":
                                Console.WriteLine("...");
                                Task.Run(() => pcs.StartClient(words[1], words[2], words[3]));
                                break;
                            case "Status":
                                Console.WriteLine("...");
                                Task.Run(() => pcs.PrintStatus());
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
