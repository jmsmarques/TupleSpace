using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        public void Add()
        {
            a++;
            Console.WriteLine("Works\n");
        }

        public void Read()
        {

        }

        public void Take()
        {

        }

        public List<IServerService> GetView()
        {
            return view;     
        }

        public void Wait(int x)
        {
        
        }
        //end of client functions
    }
}
