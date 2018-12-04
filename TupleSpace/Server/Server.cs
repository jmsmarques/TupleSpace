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
using System.Threading;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            string[] conf;
            string myRemoteObject = null;
            string serverLoc = null;
            int maxDelay = 0, minDelay = 0;
            if(args.Length == 0) //le do ficheiro
            {
                conf = ReadConfFile(); //pos 0 is port -- pos 1 is type
            }
            else //le dos argumentos
            {
                conf = ReadArgs(args);                
                minDelay = System.Convert.ToInt32(conf[3]);
                maxDelay = System.Convert.ToInt32(conf[4]);
                myRemoteObject = conf[5];
            }
            

            if (!conf[2].Equals("null"))
            {
                //serverLoc = conf[2];
                serverLoc = System.Configuration.ConfigurationManager.AppSettings["server"];
            }
            Console.WriteLine("Port:{0}\nType:{1}\nObj:{2}", conf[0], conf[1], myRemoteObject);

            BinaryServerFormatterSinkProvider provider = new BinaryServerFormatterSinkProvider();
            provider.TypeFilterLevel = TypeFilterLevel.Full;
            IDictionary props = new Hashtable();
            props["port"] = System.Convert.ToInt32(conf[0]);
            TcpChannel channel = new TcpChannel(props, null, provider);
                        
            ChannelServices.RegisterChannel(channel, true);

            ServerService mo = new ServerService(System.Convert.ToInt32(conf[1]), minDelay, maxDelay, serverLoc);

            if(serverLoc != null)
            {
                mo.Init(serverLoc);
            }

            RemotingServices.Marshal(mo,myRemoteObject,
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
            string[] result = new string[7];

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
            result[3] = args[3];
            result[4] = args[4];
            result[5] = args[5];
                 
            return result;
        }
    }
}
