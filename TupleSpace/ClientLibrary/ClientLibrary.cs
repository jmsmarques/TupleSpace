using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary
{
    public interface IServerService
    {
        void Add();

        void Read();

        void Take();

        List<IServerService> GetView();

        void Wait(int x);
    }
}
