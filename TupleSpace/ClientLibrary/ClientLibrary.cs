using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary
{
    public interface IServerService
    {
        void Freeze(bool value);

        void Add(List<string> tuple);

        List<string> Read(List<string> tuple);

        List<string> Take(List<string> tuple);

        List<IServerService> GetView();

        void Wait(int x);

        void Status();
    }  
}
