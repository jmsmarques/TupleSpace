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
        private int a;

        public int A
        {
            get { return a; }
        }

        public ServerService()
        {
            a = 1;
        }

        public void add()
        {
            a++;
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
