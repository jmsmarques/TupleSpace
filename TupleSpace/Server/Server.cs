using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Collections;
using System.Runtime.Serialization.Formatters;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 8086;
            TcpChannel channel = new TcpChannel(props, null, provider);  
            
            ChannelServices.RegisterChannel(channel, true);

            ServerService mo = new ServerService();
            RemotingServices.Marshal(mo,"MyRemoteObject",
            typeof(ServerService));

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();

            System.Console.WriteLine("{0}", mo.A);
            System.Console.ReadLine();
        }
    }
}
