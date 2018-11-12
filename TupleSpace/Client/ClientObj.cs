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
        private List<IServerService> view;
        private readonly int comType; //1 for SMR 2 for XL

        public ClientObj(List<IServerService> view, int type)
        {
            this.view = view;
            comType = type;
        }

        public void Add(String tuple)
        {
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
            List<string> takeTuple;

            takeTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {
                takeTuple = server.Take(takeTuple);
            }

            PrintTuple(takeTuple);
        }

        public void Read(String tuple)
        {
            List<string> readTuple;

            readTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {
                readTuple = server.Read(readTuple);
                if (readTuple != null)
                    break;
            }

            PrintTuple(readTuple);
        }

        private List<string> TransformToTuple(String tuple)
        {
            List<string> returnValue = new List<string>();
            tuple = tuple.Trim('<');
            tuple = tuple.Trim('>');

            int aux = 0;
            bool ignore = false;

            for(int i = 0; i < tuple.Length; i++)
            {
                if(tuple[i] == ',' && !ignore)
                {
                    returnValue.Add(tuple.Substring(aux, i - aux));
                    aux = i + 1;
                }
                else if(tuple[i] == '(')
                {
                    ignore = true;
                }
                else if (tuple[i] == ')')
                {
                    ignore = false;
                }
            }
            returnValue.Add(tuple.Substring(aux, tuple.Length - aux));

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

        private void PrintTuple(List<string> readTuple)
        {
            if (readTuple != null)
            {
                Console.Write("<");
                foreach (string s in readTuple)
                {
                    Console.Write("{0}", s);
                    if (!s.Equals(readTuple.Last()))
                    {
                        Console.Write(",");
                    }
                }
                Console.WriteLine(">");
            }
        }
    }
}
