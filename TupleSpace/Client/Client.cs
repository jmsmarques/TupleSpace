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

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {          
            TcpChannel channel = new TcpChannel();                
            ChannelServices.RegisterChannel(channel, true);

            IServerService obj = (IServerService)Activator.GetObject(
                    typeof(IServerService),
                    "tcp://localhost:8086/MyRemoteObject");

            ClientObj client = new ClientObj(obj.GetView());

            Console.WriteLine("Client\n");

            Exec(client);
        }

        private static void Exec(ClientObj client)
        {
            int loop = 1;
            string input;
            bool go = true;
            string line;
            while (go)
            {                
                input = Console.ReadLine();
                System.IO.StreamReader file = new System.IO.StreamReader(input);
                while ((line = file.ReadLine()) != null)
                {
                    System.Console.WriteLine(line);
                    string[] words = line.Split(' ');
                    switch (words[0])
                    {
                        case "add":
                            for (int i = 0; i < loop; i++)
                            {
                                client.Add(words[1]);
                            }
                            break;
                        case "read":
                            for (int i = 0; i < loop; i++)
                            {
                                client.Read(words[1]);
                            }
                            break;
                        case "take":
                            for (int i = 0; i < loop; i++)
                            {
                                client.Take(words[1]);
                            }
                            break;
                        case "wait":
                            client.Wait = System.Convert.ToInt32(words[1]);
                            break;
                        case "begin-repeat":
                            loop = System.Convert.ToInt32(words[1]);
                            break;
                        case "end-repeat":
                            loop = 1;
                            break;
                        case "exit":
                        case "Exit":
                            go = false;
                            break;
                        default:
                            Console.WriteLine("Command not recognized.\n");
                            break;
                    }
                }
                
            }
        }
    }
}
