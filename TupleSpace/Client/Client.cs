using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using ClientLibrary;

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

            ClientObj client = new ClientObj(obj.getView());

            Console.WriteLine("Client\nPress <enter> to exit...\n");
            while(true)
            {
                client.add();
                Console.ReadLine();
            }
            Console.ReadLine();
        }
    }
}
