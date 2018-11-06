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

            TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach(IServerService server in view)
            {
                server.Add();
            }
        }

        public void Take(String tuple)
        {
            Thread.Sleep(wait * 1000);
            wait = 0;

            TransformToTuple(tuple);

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

            TransformToTuple(tuple);

            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        public int Wait
        { set { this.wait = value; } }

        private void TransformToTuple(String tuple)
        {
            tuple = tuple.Trim('<');
            tuple = tuple.Trim('>');
            string[] words = tuple.Split(',');      
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
