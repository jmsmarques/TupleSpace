using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClientLibrary;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerService
    {
        private List<IServerService> view;
        private List<List<string>> tuples;  

        public ServerService()
        {
            view = new List<IServerService>();
            view.Add(this);
            tuples = new List<List<string>>();
        }

        //client functions
        public void Add(List<string> tuple)
        {
            tuples.Add(tuple);
        }

        public List<string> Read(List<string> tuple)
        {
            int aux;
            foreach(List<string> tup in tuples)
            {
                aux = 0;
                if (tup.Count == tuple.Count)
                {
                    for (int i = 0; i < tuple.Count; i++)
                    {
                        if(tup[i][0] == '\"') //string
                        {
                            if (CmpString(tup[i], tuple[i]))
                            {
                                aux++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        else //object
                        {
                            if (tuple[i].Equals("null") || tuple[i].Equals(tup[i]))
                            {
                                aux++;
                            }
                            else
                            {
                                break;
                            }
                        }
                        
                    }
                    if (aux == tuple.Count) {return tup; } 
                }
            }
            return null;
        }

        public List<string> Take(List<string> tuple)
        {            
            List<string> returnValue = Read(tuple);
            tuples.Remove(returnValue);
            return returnValue;
        }

        public List<IServerService> GetView()
        {
            return view;     
        }

        public void Wait(int x)
        {
        
        }
        //end of client functions

        private bool CmpString(string original, string toCompare) 
        {
            original = original.Trim('\"');
            toCompare = toCompare.Trim('\"');
            
            if (toCompare.Equals("*") || toCompare.Equals(original))
            {
                return true;
            }
            else if (toCompare.StartsWith("*")) //checking if original ends with toCompare
            {
                if (toCompare.Substring(1, toCompare.Length - 1).Equals(
                    original.Substring(original.Length - (toCompare.Length - 1), toCompare.Length - 1)))
                {
                    return true;
                }
            }
            else if(toCompare.EndsWith("*")) //checking if original starts with toCompare
            {
                if (toCompare.Substring(0, toCompare.Length - 1).Equals(
                    original.Substring(0, toCompare.Length - 1)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
