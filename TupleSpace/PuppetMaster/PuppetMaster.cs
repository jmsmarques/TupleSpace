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
using System.Threading;

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
            while ((line = Console.ReadLine()) != null)
            {
                //System.Console.WriteLine(line);
                string[] words = line.Split(' ');
                ReadCommand(pcs, words);
            }            
        }

        static void ExecFile(PuppetMasterServices pcs, string input)
        {
            string line;   
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(input);
                while ((line = file.ReadLine()) != null)
                {
                    //System.Console.WriteLine(line);
                    string[] words = line.Split(' ');
                    ReadCommand(pcs, words);
                    Console.ReadLine();
                }
                Console.WriteLine("File Reading Finished");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File doesn't exists");
            }
            catch(DirectoryNotFoundException)
            {
                Console.WriteLine("File doesn't exists");
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid Command");
            }               
        }        

        static void ReadCommand(PuppetMasterServices pcs, string[] words)
        {
            switch (words[0])
            {
                case "Server":
                    Console.WriteLine("Starting Server...");
                    Task.Run(() => pcs.StartServer(words[1], words[2],
                        System.Convert.ToInt32(words[3]), System.Convert.ToInt32(words[4])));
                    break;
                case "Client":
                    Console.WriteLine("Starting Client...");
                    Task.Run(() => pcs.StartClient(words[1], words[2], words[3]));
                    break;
                case "Status":
                    Console.WriteLine("Printing Status...");
                    Task.Run(() => pcs.PrintStatus());
                    break;
                case "Wait":
                    Console.WriteLine("Waiting {0}...", System.Convert.ToInt32(words[1]) / 1000);
                    Thread.Sleep(System.Convert.ToInt32(words[1]));
                    break;
                case "Crash":
                    Console.WriteLine("Crash Process...");
                    Task.Run(() => pcs.Crash(words[1]));
                    break;
                case "Freeze":
                    Console.WriteLine("Freezing Process...");
                    Task.Run(() => pcs.Freeze(words[1]));
                    break;
                case "Unfreeze":
                    Console.WriteLine("Unfreezing Process...");
                    Task.Run(() => pcs.Unfreeze(words[1]));
                    break;   
                default:
                    ExecFile(pcs, "../../../" + words[0]);
                    break;
            }
        }
    }        
}
