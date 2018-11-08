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
using System.IO;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            int[] conf;

            conf = ReadConfFile(); //pos 0 is port -- pos 1 is type

            Console.WriteLine("Port:{0}\nType:{1}", conf[0], conf[1]);

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = conf[0];
            TcpChannel channel = new TcpChannel(props, null, provider);  
            
            ChannelServices.RegisterChannel(channel, true);

            ServerService mo = new ServerService(conf[1]);
            RemotingServices.Marshal(mo,"MyRemoteObject",
            typeof(ServerService));

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }

        static int[] ReadConfFile()
        {           
            int [] result = new int [2];
            using (StreamReader file = File.OpenText("../../../serverConf.txt"))
            {
                string line;
                string[] words;
                while ((line = file.ReadLine()) != null)
                {
                    words = line.Split(':');
                    if(words[0].Equals("Port"))
                    {
                        result[0] = System.Convert.ToInt32(words[1]);
                    }
                    else if(words[0].Equals("Type"))
                    {
                        if(words[1].Equals("SMR"))
                        {
                            result[1] = 1;
                        }
                        else if (words[1].Equals("XL"))
                        {
                            result[1] = 2;
                        }
                        else
                        {
                            //error
                        }
                    }
                }
            }

            return result;
        }
    }
}
