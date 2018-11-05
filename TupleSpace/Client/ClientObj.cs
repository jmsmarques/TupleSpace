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

        public void Add()
        {
            if(CompareView())
            {
                view = view[0].GetView();
            }
            foreach(IServerService server in view)
            {
                server.Add();
            }
        }

        public void Take()
        {
            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        public void Read()
        {
            if (CompareView())
            {
                view = view[0].GetView();
            }
            foreach (IServerService server in view)
            {

            }
        }

        public void Wait()
        {

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
