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
        private List<IServerService> view;

        public int A
        {
            get { return a; }
        }

        public ServerService()
        {
            a = 1;
            view = new List<IServerService>();
            view.Add(this);
        }

        //client functions
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

        public List<IServerService> getView()
        {
            return view;     
        }
        //end of client functions
    }
}
