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

        int XlRequest(List<string> tuple, string id, string req);

        List<string> XlConfirmation(string id, int nr);

        void Add(List<string> tuple);

        List<string> Read(List<string> tuple);

        List<string> Take(List<string> tuple);

        List<IServerService> GetView();

        void Status();
    }  
}
