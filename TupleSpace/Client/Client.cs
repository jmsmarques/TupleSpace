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
            string input;
            bool go = true;
            while (go)
            {                
                input = Console.ReadLine();
                string[] words = input.Split(' ');
                switch (words[0])
                {
                    case "add":
                        client.Add();
                        break;
                    case "read":
                        client.Read();
                        break;
                    case "take":
                        client.Take();
                        break;
                    case "wait":
                        client.Wait();
                        break;
                    case "begin-repeat":
                        Console.WriteLine("begin-repeat " + words[1] + " times ");
                        break;
                    case "end-repeat":
                        Console.WriteLine("end-repeat");
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
