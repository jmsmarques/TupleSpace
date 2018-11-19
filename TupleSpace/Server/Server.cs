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
            string[] conf;
            string serverLoc = null;
            if(args.Length == 0) //le do ficheiro
            {
                conf = ReadConfFile(); //pos 0 is port -- pos 1 is type
            }
            else //le dos argumentos
            {
                conf = ReadArgs(args);

            }
            

            if (!conf[2].Equals("null"))
            {
                serverLoc = "tcp://" + conf[2] + ":" + conf[3] + "/MyRemoteObject";
            }
            Console.WriteLine("Port:{0}\nType:{1}", conf[0], conf[1]);

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = System.Convert.ToInt32(conf[0]);
            TcpChannel channel = new TcpChannel(props, null, provider);
                        
            ChannelServices.RegisterChannel(channel, true);

            ServerService mo = new ServerService(System.Convert.ToInt32(conf[1]));

            if(serverLoc != null)
            {
                mo.Init(serverLoc);
            }

            RemotingServices.Marshal(mo,"MyRemoteObject",
            typeof(ServerService));

            System.Console.WriteLine("<enter> para sair...");
            System.Console.ReadLine();
        }

        static string[] ReadConfFile()
        {           
            string [] result = new string [4];
            using (StreamReader file = File.OpenText("../../../serverConf.txt"))
            {
                string line;
                string[] words;
                while ((line = file.ReadLine()) != null)
                {
                    words = line.Split(':');
                    if(words[0].Equals("Port"))
                    {
                        result[0] = words[1];
                    }
                    else if(words[0].Equals("Type"))
                    {
                        if(words[1].Equals("SMR"))
                        {
                            result[1] = "1";
                        }
                        else if (words[1].Equals("XL"))
                        {
                            result[1] = "2";
                        }
                        else
                        {
                            //error
                        }
                    }
                    else if (words[0].Equals("ViewServer"))
                    {
                        result[2] = words[1];
                    }
                    else if (words[0].Equals("ViewPort"))
                    {
                        result[3] = words[1];
                    }
                }
            }

            return result;
        }

        private static string[] ReadArgs(string[] args)
        {
            string[] result = new string[4];

            result[0] = args[0];
           
            if (args[1].Equals("SMR"))
            {
                result[1] = "1";
            }
            else if (args[1].Equals("XL"))
            {
                result[1] = "2";
            }

            result[2] = args[2];

            if (result[2].Equals("null"))
            {
                result[3] = null;
            }
            else
            {
                result[3] = args[3];
            }      
            return result;
        }
    }
}
