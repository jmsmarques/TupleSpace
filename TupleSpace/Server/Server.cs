using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            /*BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = 8086;
            TcpChannel channel = new TcpChannel(props, null, provider);  
            */ 

            TcpChannel channel = new TcpChannel(8086);
            ChannelServices.RegisterChannel(channel, true);

            RemotingConfiguration.RegisterWellKnownServiceType(
                typeof(ServerService),
                "MyRemoteObject",
                WellKnownObjectMode.Singleton);

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }
    }
}
