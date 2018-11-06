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
        private List<IServerService> view;
        private List<List<string>> tuples;  

        public ServerService()
        {
            view = new List<IServerService>();
            view.Add(this);
            tuples = new List<List<string>>();
        }

        //client functions
        public void Add(List<string> tuple)
        {
            tuples.Add(tuple);
        }

        public List<string> Read(List<string> tuple)
        {
            int aux;
            foreach(List<string> tup in tuples)
            {
                aux = 0;
                if (tup.Count == tuple.Count)
                {
                    for (int i = 0; i < tuple.Count; i++)
                    {
                        if (tuple[i].Equals("*") || tuple[i].Equals(tup[i]))
                        {
                            aux++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (aux == tuple.Count) {return tup; } 
                }
            }
            return null;
        }

        public List<string> Take(List<string> tuple)
        {
            return null;
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
