using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class Pcs
    {
        static void Main(string[] args)
        {
            TcpChannel channel = new TcpChannel(10000);
            ChannelServices.RegisterChannel(channel, true);


            PcsService mo = new PcsService(args[0], args[1], args[2]); //needs to be fixed


            RemotingServices.Marshal(mo,"PcsService",
                typeof(PcsService));

            Console.WriteLine("PCS");
            Console.WriteLine("Press <enter> to exit...");
            Console.ReadLine();
        }
    }


}
