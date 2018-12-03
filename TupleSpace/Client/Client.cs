using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using ClientLibrary;
using System.Threading;
using System.IO;

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            string[] aux;
            int comType;

            if(args.Length == 0) //read from file
            {
                aux = ReadConfFile();
            }
            else //arguments
            {
                aux = ReadArgs(args);
            }

            string serverLoc = "tcp://" + aux[0] + "/MyRemoteObject";

            if(aux[1].Equals("SMR"))
            {
                comType = 1;
            }
            else if (aux[1].Equals("XL"))
            {
                comType = 2;
            }
            else
            {
                //error
                comType = 0;
            }

            Console.WriteLine(serverLoc);

            TcpChannel channel = new TcpChannel();
            ChannelServices.RegisterChannel(channel, true);

            IServerService obj = (IServerService)Activator.GetObject(
                    typeof(IServerService),
                    serverLoc);

            ClientObj client = new ClientObj(obj.GetView(), comType);

            Console.WriteLine("Client\n");

            if (args.Length < 3)
                Exec(client);
            else
                ExecPuppet(client, args[2]);

            Console.WriteLine("Script finished");

            Console.ReadLine();
        }

        private static void Exec(ClientObj client)
        {
            string line;            
            try
            {
                while ((line = Console.ReadLine()) != null)
                {
                    System.Console.WriteLine(line);
                    string[] words = line.Split(' ');
                    switch (words[0])
                    {
                        case "begin-repeat":
                            BeginRepeat(client, System.Convert.ToInt32(words[1]));
                            break;
                        case "exit":
                        case "Exit":
                            break;
                        default:
                            ReadCommand(client, words[0], words[1]);
                            break;
                    }
                }
            } catch(FileNotFoundException)
            {
                Console.WriteLine("File doesn't exists");
            }
        }

        private static void ExecPuppet(ClientObj client, string input)
        {
            string line;
            try
            {
                System.IO.StreamReader file = new System.IO.StreamReader(input);
                while ((line = file.ReadLine()) != null)
                {
                    System.Console.WriteLine(line);
                    string[] words = line.Split(' ');
                    switch (words[0])
                    {
                        case "begin-repeat":
                            BeginRepeat(client, System.Convert.ToInt32(words[1]));
                            break;
                        case "exit":
                        case "Exit":
                            break;
                        default:
                            ReadCommand(client, words[0], words[1]);
                            break;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File doesn't exists");
            }
        }

        private static void ReadCommand(ClientObj client, string command, string parameters)
        {
            switch (command)
            {
                case "add":
                    client.Add(parameters);
                    break;
                case "read":
                    client.Read(parameters);
                    break;
                case "take":
                    client.Take(parameters);
                    break;
                case "wait":
                    Thread.Sleep(System.Convert.ToInt32(parameters));
                    break;
                default:
                    Console.WriteLine("Command not recognized.\n");
                    break;
            }
        }

        private static void BeginRepeat(ClientObj client, int loop)
        {
            string line;
            bool end = false;
            List<string[]> commands = new List<string[]>();

            while ((line = Console.ReadLine()) != null && !end)
            {
                //System.Console.WriteLine(line);
                string[] words = line.Split(' ');
                switch (words[0])
                {
                    case "end-repeat":
                        end = true;
                        break;
                    default:
                        commands.Add(words);
                        //ReadCommand(client, words[0], words[1]);
                        break;
                }
            }

            for(int i = 0; i < loop; i++)
            {
                for(int n = 0; n < commands.Count; n++)
                    ReadCommand(client,commands[n][0], commands[n][1]);
            }
        }

        private static string [] ReadConfFile()
        {
            string[] aux = new string[3];
            using (StreamReader file = File.OpenText("../../../clientConf.txt"))
            {
                string line;
                string[] words;
                while ((line = file.ReadLine()) != null)
                {
                    words = line.Split(':');
                    if (words[0].Equals("Server"))
                    {
                        aux[0] = words[1] + ":" + words[2];
                    } 
                    else if (words[0].Equals("Type"))
                    {
                        if (words[1].Equals("SMR"))
                        {
                            aux[1] = "SMR";
                        }
                        else if (words[1].Equals("XL"))
                        {
                            aux[1] = "XL";
                        }
                        else
                        {
                            //error
                        }
                    }
                }
            }
            return aux;
        }

        private static string[] ReadArgs(string[] args)
        {
            string[] result = new string[3];

            result[0] = args[0];

            if (args[1].Equals("SMR"))
            {
                result[1] = "SMR";
            }
            else if (args[1].Equals("XL"))
            {
                result[1] = "XL";
            }

            return result;
        }
    }
}
