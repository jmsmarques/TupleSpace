using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClientLibrary;

namespace Client
{
    class ClientObj
    {
        private List<IServerService> view;

        public ClientObj(List<IServerService> view)
        {
            this.view = view;
        }

        public void add()
        {
            if(compareView())
            {
                view = view[0].getView();
            }
            foreach(IServerService server in view)
            {
                server.add();
            }
        }

        public void take()
        {
            if (compareView())
            {
                view = view[0].getView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        public void read()
        {
            if (compareView())
            {
                view = view[0].getView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        private bool compareView()
        {
            List<IServerService> aux;
            aux = view[0].getView();
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
