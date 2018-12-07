using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class XlObj
    {
        private string req;
        private int nr;
        private string id;
        private bool agreed;
        private List<string> tuple;

        public string Req { get { return req; } }
        public int Nr { get { return nr; } }
        public string Id { get { return id; } }
        public bool Agreed { get { return agreed; }
            set { agreed = value; } }
        public List<string> Tuple { get { return tuple; } }

        public XlObj(string req, int nr, string id, bool agreed, List<string> tuple)
        {
            this.req = req;
            this.nr = nr;
            this.id = id;
            this.agreed = agreed;
            this.tuple = tuple;
        }
    }
}
