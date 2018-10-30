using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLibrary;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerService
    {
        public void add()
        {
            Console.WriteLine("Works\n");
        }

        public void read()
        {

        }

        public void take()
        {

        }
    }
}
