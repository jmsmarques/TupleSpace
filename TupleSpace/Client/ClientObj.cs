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
        private string id;

        public ClientObj(List<IServerService> view, int type, string id)
        {
            this.view = view;
            comType = type;
            this.id = id;
        }

        public void XlRequest(string req, List<string> tuple)
        {
            Task[] tasks = new Task[view.Count];
            int[] answers = new int[view.Count];
            int i = 0;
            foreach (IServerService s in view)
            {
                Task t = Task.Run(() =>  answers[i] = s.XlRequest(tuple, id, req));
                tasks[i] = t;
                if(i < view.Count - 1) //protection because of tasks
                    i++;
            }

            Task.WaitAll(tasks);

            int max = 0;
            for (int n = 0; n < answers.Length; n++)
            {
                if (answers[n] > max)
                    max = answers[n];
            }

            i = 0;
            List<string> result = null;
            foreach(IServerService s in view)
            {
                Task t = Task.Run(() => result = s.XlConfirmation(id, max));
                tasks[i] = t;
                i++;
            }
            if(req.Equals("Take"))
            {
                Task.WaitAll(tasks);
                PrintTuple(result);
            }
            else if (req.Equals("Read"))
            {
                Task.WaitAny(tasks);
                PrintTuple(result);
            }
            
        } 

        public void Add(String tuple)
        {
            List<string> addTuple;

            addTuple = TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }

            if (comType == 2)
            {
                /*Task[] tasks = new Task[view.Count];
                int i = 0;
                foreach (IServerService server in view)
                {
                    Task t = Task.Run(() => server.Add(addTuple));
                    tasks[i] = t;
                    i++;
                }
                Task.WaitAll(tasks);*/

                XlRequest("Add", addTuple);
            }
            else if (comType == 1)
            {
                view[0].Add(addTuple);  
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
            if( comType == 1)
            {
                takeTuple = view[0].Take(takeTuple);
            }
            else if (comType == 2)
            {
                Task[] tasks = new Task[view.Count];
                int i = 0;
                foreach (IServerService server in view)
                {
                    Task t = Task.Run(() => takeTuple = server.Take(takeTuple));
                    tasks[i] = t;
                    i++;
                }
                Task.WaitAll(tasks);
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

            if (comType == 1)
            {
                readTuple = view[0].Read(readTuple);
                PrintTuple(readTuple);
            }
            
            else if(comType == 2)
            {
                /*Task[] tasks = new Task[view.Count];
                int i = 0;
                foreach (IServerService server in view)
                {
                    Task t = Task.Run(() => readTuple = server.Read(readTuple));
                    tasks[i] = t;
                    i++;
                }
                Task.WaitAny(tasks);*/

                XlRequest("Read", readTuple);
            }            
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
