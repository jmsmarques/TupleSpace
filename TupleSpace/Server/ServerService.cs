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
        private string loc;
        private readonly int comType; //1 for SMR 2 for XL
        private static object _lock = new object();
        private static int frozen_req= 0;
        private static int test= 2;
        private int maxDelay, minDelay;
        private int currentSeqNum = 0;

        private Random rnd = new Random();
        public Estado state = Estado.LEADER;
        public bool leader = true;

        private bool freeze;

        public string Loc { get { return loc; } }
        public void IncSeqNum(){ currentSeqNum++;} 


        public enum Estado {LEADER,CANDIDATE,FOLLOWER};

        public ServerService(int comType, int min, int max, string loc)
        {            
            view = new List<IServerService>();            
            view.Add(this);
            tuples = new List<List<string>>();
            this.comType = comType;
            maxDelay = max;
            minDelay = min;
            this.loc = loc;
            freeze = false;
        }

        //server functions
        public void Init(string serverLoc)
        {
            leader = false;
            state = Estado.FOLLOWER;
            ServerService obj = (ServerService)Activator.GetObject(
                    typeof(ServerService),
                    serverLoc);

            obj.GetServerView(this);

        }

        public void GetServerView(ServerService newServer)
        {
            view.Add(newServer);
            foreach (ServerService serv in view)
            {
                serv.setServerView(view);
                Console.WriteLine("View sent");
            }
        }
        public void setServerView(List<IServerService> newView)
        {
            view = newView;
            Console.WriteLine(view.Count);
        }
        //end of server functions

        //client functions
        public void Freeze(bool value)
        {
            this.freeze = value;
        }

        public void Add(List<string> tuple)
        {   
            Console.WriteLine("[DEBUG] ADD Request");
            Console.WriteLine("[DEBUG] Start Sleep");
            Thread.Sleep(rnd.Next(minDelay, maxDelay));
            Console.WriteLine("[DEBUG] END Sleep");
            

            while (freeze) ;
            if(comType == 1)
            {
                Console.WriteLine("[DEBUG] ADD is a SMR");   
                //SMR
                AddSMR(tuple,currentSeqNum + 1);
            }
            else if(comType==2){
                //XL
                lock (_lock)
                    {
                        tuples.Add(tuple);
                        //test += 2;
                        //Console.WriteLine(test);
                }
                if (frozen_req > 0)
                {
                    lock (this){
                        Monitor.PulseAll(this);
                    }
                }
            }
        }

        public void AddSMR(List<string> tuple, int SeqNum) {
            Console.WriteLine("[DEBUG] AddSMR Request");   
            while (true) {
                Console.WriteLine("[DEBUG] AddSMR While Iteration");   
                if (currentSeqNum + 1 == SeqNum) {
                    Console.WriteLine("[DEBUG] AddSMR Correct Sequence Number");   
                    IncSeqNum();
                  

                    if (leader)
                    {   
                        Console.WriteLine("[DEBUG] AddSMR is the Leader");   
                        ReplicateAdd(tuple, currentSeqNum);
                    }
                    lock (_lock)
                    {
                        tuples.Add(tuple);
                        //test += 2;
                        //Console.WriteLine(test);
                    }
                    if (frozen_req > 0)
                    {
                        lock (this) {
                            Monitor.PulseAll(this);
                        }
                    }
                    break;
                }
                else{
                    lock (this) {
                        Console.WriteLine("[DEBUG] Wrong Sequence Number: Expected: "+(currentSeqNum+1)+"Obtained: "+SeqNum);   
                        Monitor.Wait(this);
                    }
                }

            }
        }
        public void ReplicateAdd(List<string> tuple,int SeqNum)
        {
            foreach (ServerService serv in view)
            {
                if (!serv.leader)
                {
                    Console.WriteLine("[DEBUG] ReplicateAdd to Follower");   
                    serv.AddSMR(tuple,SeqNum);
                }
            }           
        }

        public List<string> Read(List<string> tuple)
        {

            Thread.Sleep(rnd.Next(minDelay,maxDelay));
            while (freeze) ;
            List<string> returnValue = null;
            returnValue = ReadAux(tuple);
            while(returnValue == null)
            {
                Console.WriteLine("bloqueado");
                frozen_req++;
                lock(this){
                    Monitor.Wait(this);
                }
                returnValue = ReadAux(tuple);
                frozen_req--;
            }
           
            return returnValue;
        }
        public List<string> ReadAux(List<string> tuple)
        {
            lock (_lock)
            {
                int aux;
                foreach (List<string> tup in tuples)
                {
                    aux = 0;
                    if (tup.Count == tuple.Count)
                    {
                        for (int i = 0; i < tuple.Count; i++)
                        {
                            if (tup[i][0] == '\"') //string
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
                                if (tuple[i].Equals("null") || tuple[i].Equals(tup[i])
                                    || CmpObjectType(tuple[i], tup[i]))
                                {
                                    aux++;
                                }
                                else
                                {
                                    break;
                                }
                            }

                        }
                        if (aux == tuple.Count) { return tup; }
                    }
                }
            }
            return null;         
        }

        public List<string> Take(List<string> tuple)
        {
            Console.WriteLine("[DEBUG] Take Requested"); 
            Console.WriteLine("[DEBUG] Start Sleep");
            Thread.Sleep(rnd.Next(minDelay, maxDelay));
            Console.WriteLine("[DEBUG] END Sleep");
            List<string> returnValue = null;
           
            while (freeze) ;



            if (comType == 1)
            {
                Console.WriteLine("[DEBUG] Take is a SMR");   
                //SMR
                return TakeSMR(tuple, currentSeqNum + 1);
            }

            returnValue = ReadAux(tuple);
            while (returnValue == null)
            {
                Console.WriteLine("bloqueado");
                frozen_req++;
                lock (this)
                {
                    Monitor.Wait(this);
                }
                returnValue = ReadAux(tuple);
                frozen_req--;
            }
            lock (_lock)
            {
                tuples.Remove(returnValue);
            }
            
            return returnValue;
        }

        public List<String> TakeSMR(List<string> tuple, int SeqNum)
        {
            Console.WriteLine("[DEBUG] TakeSMR Requested");  
            List<string> returnValue = null;
            while (true)
            {
                Console.WriteLine("[DEBUG] AddSMR While Iteration"); 
                if (currentSeqNum + 1 == SeqNum)
                {
                    Console.WriteLine("[DEBUG] TakeSMR SeqNum Expected"); 
                    IncSeqNum();

                    if (leader)
                    {
                        Console.WriteLine("[DEBUG] AddSMR is LEADER"); 
                        ReplicateTake(tuple);
                    }
                    returnValue = ReadAux(tuple);
                    while (returnValue == null)
                    {
                        Console.WriteLine("bloqueado");
                        frozen_req++;
                        lock (this)
                        {
                            //IncSeqNum();
                            Monitor.Wait(this);
                        }
                        returnValue = ReadAux(tuple);
                        frozen_req--;
                    }
                    return returnValue;
                }
                else
                {
                    lock (this)
                    {
                        Console.WriteLine("[DEBUG] Wrong Sequence Number: Expected: "+(currentSeqNum+1)+"Obtained: "+SeqNum);
                        Monitor.Wait(this);
                    }
                }
            }
        }

        public void ReplicateTake(List<string> tuple)
        {
            foreach (ServerService serv in view)
            {
                if (!serv.leader)
                {
                    Console.WriteLine("[DEBUG] Replicate Take to Follower");   
                    serv.TakeSMR(tuple,currentSeqNum);
                }
            }
        }

        public List<IServerService> GetView()
        {
            return view;     
        }

        public void Wait(int x)
        {
        
        }
        //end of client functions

        public void Status()
        {
            /*Console.WriteLine("View");
            foreach(IServerService s in view)
            {
                Console.WriteLine(s.Loc);
            }
            Console.WriteLine("End of view");*/

            Console.WriteLine("Tuples");
            foreach(List<string> tuple in tuples)
            {
                PrintTuple(tuple);
            }
            Console.WriteLine("End of tuples");
        }        

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

        private bool CmpObjectType(string tuple, string tup)
        {
            for (int n = 0; n < tuple.Length; n++)
            {
                if (tuple[n] == '(')
                {
                    return false;
                    
                }
            }
            if (tuple.Equals(tup.Substring(0, tuple.Length)))
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
