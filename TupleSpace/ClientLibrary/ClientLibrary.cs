using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientLibrary
{
    public interface IServerService
    {
        void Add(List<string> tuple);

        List<string> Read(List<string> tuple);

        List<string> Take(List<string> tuple);

        List<IServerService> GetView();

        void Wait(int x);
    }

    public interface ITeste
    {
        void PrintStatus();
    }
}
