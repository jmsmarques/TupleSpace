using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientLibrary;

namespace Client
{
    class ClientObj
    {
        private int wait;
        private List<IServerService> view;

        public ClientObj(List<IServerService> view)
        {
            this.view = view;
        }

        public void Add(String tuple)
        {            
            Thread.Sleep(wait * 1000);
            wait = 0;

            List<string> addTuple;

            addTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }

            foreach(IServerService server in view)
            {
                server.Add(addTuple);
            }
        }

        public void Take(String tuple)
        {
            Thread.Sleep(wait * 1000);
            wait = 0;

            List<string> takeTuple;

            takeTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        public void Read(String tuple)
        {
            Thread.Sleep(wait * 1000);
            wait = 0;

            List<string> readTuple;

            readTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {
                readTuple = server.Read(readTuple);
            }

            if (readTuple != null)
            {
                Console.Write("<");
                foreach (string s in readTuple)
                {
                    Console.Write("{0}", s);
                    if(!s.Equals(readTuple.Last()))
                    {
                        Console.Write(",");
                    }
                }
                Console.Write(">");
            }
        }

        public int Wait
        { set { this.wait = value; } }

        private List<string> TransformToTuple(String tuple)
        {
            List<string> returnValue = new List<string>();
            tuple = tuple.Trim('<');
            tuple = tuple.Trim('>');
            string[] words = tuple.Split(',');

            for(int i = 0; i < words.Length; i++)
            {
                returnValue.Add(words[i]);
            }

            return returnValue;
        }

        private bool CompareView()
        {
            List<IServerService> aux;
            aux = view[0].GetView();
            if (aux.Count == view.Count)
            {
                for (int i = 0; i < aux.Count; i++)
                {
                    if (!aux[i].Equals(view[i]))
                    {
                        return true;
                    }
                }
            }
            else
            {
                return true;
            }
            return false;
        }
    }
}
