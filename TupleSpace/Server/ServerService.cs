using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using ClientLibrary;

namespace Server
{
    class ServerService : MarshalByRefObject, IServerService
    {
        private int i = 1;
        private static List<IServerService> view;
        private List<List<string>> tuples;
        private List<Object[]> holdBackQueue;
        private List<Object[]> deliverQueue;
        private int p, a;
        private string loc;
        private readonly int comType; //1 for SMR 2 for XL
        private static object _lock = new object();

        private object SMRlock = new object();
        private static object _lock1 = new object();
       
        private static int frozen_req= 0;
        private static int test= 2;
        private int maxDelay, minDelay;
        private string id;
        private int currentSeqNum = 0;
        private int servSeqNum = 0;
        public string getID() {
            return id;
        }
        private Random rnd = new Random();

        private static System.Timers.Timer electionTimer;
        private static System.Timers.Timer heartbeatTimer;
        public static Estado state;
        private static int term = 1;

        public enum Estado { LEADER, CANDIDATE, FOLLOWER };

        public bool leader = true;

        private bool freeze;

        public string Loc { get { return loc; } }
       

        public ServerService(int comType, int min, int max, string loc, string id)
        {            
            view = new List<IServerService>();            
            view.Add(this);
            tuples = new List<List<string>>();
            this.comType = comType;
            maxDelay = max;
            minDelay = min;
            this.loc = loc;
            freeze = false;
            this.id = id;

            Console.WriteLine("MEU ID:" + id);

            if(comType == 2)
            {
                holdBackQueue = new List<Object[]>();
                deliverQueue = new List<Object[]>();
                p = a = 0;
            }
        }

        //server functions
        public void Init(string serverLoc)
        {
            leader = false;
            ServerService obj = (ServerService)Activator.GetObject(
                    typeof(ServerService),
                    serverLoc);

            obj.GetServerInfo(this);

        }

        public void GetServerInfo(ServerService newServer)
        {
            view.Add(newServer);
            foreach (ServerService serv in view)
            {
                serv.SetServerView(view);
                Console.WriteLine("servReq: " + servSeqNum + ", currentSeq: " + currentSeqNum);
                Status();
                serv.SetInfo(servSeqNum, currentSeqNum, tuples);
                Console.WriteLine("View sent");
            }
        }

        public void SetServerView(List<IServerService> newView)
        {
            Console.WriteLine("-----------------");
            foreach(ServerService serv in newView)
            {
                Console.WriteLine(serv.getID());
            }
            Console.WriteLine("-----------------");
            view = newView;
            Console.WriteLine(view.Count);
        }

        public void SetInfo(int serverReq, int currentReq, List<List<string>> tupleSpace)
        {
            servSeqNum = serverReq;
            currentSeqNum = currentReq;
            tuples = tupleSpace;
        }
        //end of server functions

        //client functions
        public void Freeze(bool value)
        {
            this.freeze = value;
        }

        public int XlRequest(List<string> tuple, string id, string req)
        {            
            p = Math.Max(p , a) + 1;
            AddToHoldQueue(id, p, req, tuple);

            return p;
        }

        public List<string> XlConfirmation(string id, int nr)
        {            
            if(a != nr)
            {
                a = Math.Max(nr, a);
            }
            CheckChange(id, nr);

            while(true)
            {
                if ((bool)holdBackQueue[0][3] && id.Equals((string)holdBackQueue[0][2]))
                {
                    deliverQueue.Add(holdBackQueue[0]);
                    holdBackQueue.Remove(holdBackQueue[0]);
                    break;
                }                
            }

            List<string> result = null;
            while (!id.Equals((string)deliverQueue[0][2])) ;
            lock (_lock1) {
                if (((string)deliverQueue[0][0]).Equals("Add"))
                {
                    Add((List<string>)deliverQueue[0][4]);
                }
                else if (((string)deliverQueue[0][0]).Equals("Read"))
                {
                    result = Read((List<string>)deliverQueue[0][4]);
                }
                else if (((string)deliverQueue[0][0]).Equals("Take"))
                {
                    result = Take((List<string>)deliverQueue[0][4]);
                }
                deliverQueue.Remove(deliverQueue[0]);
            }            
            return result;
        }

        public void Add(List<string> tuple)
        {
           
            Console.WriteLine("[DEBUG] ADD Request");
            Console.WriteLine("[DEBUG] Start Sleep");
            Thread.Sleep(rnd.Next(minDelay, maxDelay));
            Console.WriteLine("[DEBUG] END Sleep");
            int n;
            
            


            while (freeze) ;
            if(comType == 1)
            {
                Console.WriteLine("[DEBUG] ADD is a SMR");
                //SMR
                currentSeqNum++;
                n = currentSeqNum;
                AddSMR(tuple,n);
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
            Console.WriteLine("[DEBUG]"+SeqNum+" AddSMR "); PrintTuple(tuple); Console.WriteLine(" Requested with SeqNum: " + SeqNum);
            Console.WriteLine("[DEBUG]" + SeqNum + " Start Sleep");
            Thread.Sleep(rnd.Next(minDelay, maxDelay));
            Console.WriteLine("[DEBUG]" + SeqNum + " END Sleep");
            while (true) {
                Console.WriteLine("[DEBUG]" + SeqNum + " AddSMR While Iteration");   
                if (servSeqNum+1  == SeqNum) {
                    Console.WriteLine("[DEBUG] " + SeqNum + "AddSMR Correct Sequence Number");
                   // Console.WriteLine("[DEBUG]currentSeqNum= " + currentSeqNum);
                   
                        //currentSeqNum++;
                    Console.WriteLine("[DEBUG]" + SeqNum + "servSeqNum= " + servSeqNum);

                    lock (_lock)
                    {
                        tuples.Add(tuple);
                        //servSeqNum++;
                        //test += 2;
                        //Console.WriteLine(test);
                    }

                    lock (this)
                    {
                        servSeqNum++;
                        Console.WriteLine("ServSeqNum: "+servSeqNum);
                        Monitor.PulseAll(this);
                    }

                    if (leader)
                    {   
                        Console.WriteLine("[DEBUG]" + SeqNum + " AddSMR is the Leader");   
                        ReplicateAdd(tuple, SeqNum);
                    }
                   
                    break;
                }
                else{
                    lock (this)
                    {
                        Monitor.PulseAll(this);
                    }
                    lock (this) {
                        Console.WriteLine("[DEBUG]" + SeqNum + " Wrong Sequence Number: Expected: " + (servSeqNum+1)+"Obtained: "+SeqNum);
                        //frozen_req++;
                        Monitor.Wait(this);
                          

                    }
                    //frozen_req--;

                    Console.WriteLine("[DEBUG]" + SeqNum + " RELEASE");
                }

            }
          //  if (frozen_req > 0)
           // {
                lock (this)
                {
                    Monitor.PulseAll(this);
                }
            //}
            
            Console.WriteLine(SeqNum + " FINITO");
        }

        public void ReplicateAdd(List<string> tuple,int SeqNum)
        {
            int n = SeqNum;
            foreach (ServerService serv in view)
            {
                if (!serv.leader)
                {
                    Console.WriteLine("[DEBUG] ReplicateAdd to Follower");   
                    serv.AddSMR(tuple,n);
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
            int n;
            List<string> returnValue = null;
           
            while (freeze) ;



            if (comType == 1)
            {
                Console.WriteLine("[DEBUG] Take is a SMR");
                //SMR
                currentSeqNum++;

                n = currentSeqNum;
                returnValue = TakeSMR(tuple,n);
                return returnValue;

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
            int incFlag = 1;
            Console.WriteLine("[DEBUG]" + SeqNum + " TakeSMR "); PrintTuple(tuple); Console.WriteLine(" Requested with SeqNum: " + SeqNum);
            Console.WriteLine("[DEBUG]" + SeqNum + "TAKE  Start Sleep");
            Thread.Sleep(rnd.Next(minDelay, maxDelay));
            Console.WriteLine("[DEBUG]" + SeqNum + "TAKE END sleep");
            List<string> returnValue = null;
            while (true)
            {
                Console.WriteLine("[DEBUG]" + SeqNum + " TakeSMR While Iteration"); 
                if (servSeqNum + 1 == SeqNum)
                {
                    Console.WriteLine("[DEBUG]" + SeqNum + " TakeSMR SeqNum Expected");
                  //  Console.WriteLine("[DEBUG]currentSeqNum= " + currentSeqNum);

                  //  currentSeqNum++;
                   // n = currentSeqNum;
                    Console.WriteLine("[DEBUG]" + SeqNum + "servSeqNum= " + servSeqNum);

                    returnValue = ReadAux(tuple);
                    while (returnValue == null)
                    {
                        //Status();
                        Console.WriteLine("" + SeqNum + "take bloqueado");
                        servSeqNum++;
                        Console.WriteLine("ServSeqNum: " + servSeqNum);
                        incFlag = 0;
                        lock (this)
                        {
                            Monitor.PulseAll(this);
                        }
                        lock (this)
                        {
                            frozen_req++;
                            //IncSeqNum();
                            Monitor.Wait(this);
                        }
                        frozen_req--;
                        Console.WriteLine("" + SeqNum + "SAI DO bloqueado");
                        returnValue = ReadAux(tuple);
                        
                    }
                    //lock (this)
                    //{
                    //    Monitor.PulseAll(this);
                    //}
                    if (leader)
                    {
                        Console.WriteLine("[DEBUG]" + SeqNum + " TakeSMR is LEADER"); 
                        ReplicateTake(tuple,SeqNum);
                    }
                    lock (this)
                    {
                        Monitor.PulseAll(this);
                    }
                    Console.WriteLine(SeqNum + " FINITO");
                    if (incFlag == 1) { servSeqNum++; Console.WriteLine("ServSeqNum: " + servSeqNum); }
                    //Status();
                    lock (_lock)
                    {

                        tuples.Remove(returnValue);
                        Console.WriteLine("------------------------");
                        Status();
                        Console.WriteLine("------------------------");
                    }
                    return returnValue;
                }
                else
                {
                    lock (this)
                    {
                        Console.WriteLine("[DEBUG]" + SeqNum + " Wrong Sequence Number: Expected: " + (servSeqNum+1)+"Obtained: "+SeqNum);
                        Monitor.Wait(this);
                    }
                }
            }
            
        }

        public void ReplicateTake(List<string> tuple, int seqNumber)
        {
            int n = seqNumber;
            foreach (ServerService serv in view)
            {
                if (!serv.leader)
                {
                    Console.WriteLine("[DEBUG] Replicate Take to Follower");   
                    serv.TakeSMR(tuple,n);
                }
            }
        }

        public List<IServerService> GetView()
        {
            Console.WriteLine("FDS PUTA QUE PARIU");
            foreach(ServerService serv in view)
            {
                Console.WriteLine(serv.getID());
            }
            return view;     
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

        private void AddToHoldQueue(string id, int nr, string req, List<string> tuple)
        {
            Object[] queue = new Object[5];
            //Action a;
            queue[0] = req;
            queue[1] = nr;
            queue[2] = id;
            queue[3] = false;
            queue[4] = tuple;            

            CheckPosition(nr, queue);
            
        }

        private void CheckPosition(int nr, Object[] queue)
        {
            int aux = 0;
            foreach (Object[] obj in holdBackQueue)
            {
                if ((int)obj[1] < nr)
                {
                    holdBackQueue.Insert(aux, queue);
                    break;
                }
                else if((int)obj[1] == nr)
                {
                    if(String.CompareOrdinal((string)obj[2], (string)queue[2]) > 0) //compares id for tiebreaker
                    {                    
                        holdBackQueue.Insert(aux, queue);
                        break;
                    }                    
                }
                aux++;
            }

            if (aux == holdBackQueue.Count)
            {
                holdBackQueue.Add(queue);
            }
        }

        private void CheckChange(string id, int nr)
        {
            Object[] aux = null;
            foreach(Object[] obj in holdBackQueue)
            {
                if(((string)obj[2]).Equals(id))
                {
                    obj[3] = true;
                    if ((int)obj[1] != nr)
                    {
                        aux = obj;                        
                        holdBackQueue.Remove(obj);
                    }                    
                    break;
                }
            }
            if(aux != null)
                CheckPosition(nr, aux);

        }

        public void SetTimer()
        {
            electionTimer = new System.Timers.Timer(rnd.Next(5000,10000));
            electionTimer.Elapsed += StartElection;
            electionTimer.AutoReset = true;
            electionTimer.Enabled = true;

           

        }
        private  void StartElection(Object source, ElapsedEventArgs e)
        {
             i = 1;
            Console.WriteLine("Começou Eleição!");
          /*  foreach (ServerService serv in view)
         /*   {
                
                Console.WriteLine(serv.getID());
            }*/
            state = Estado.CANDIDATE;
            term++;
            Console.WriteLine("TERM: " + term);
            Console.WriteLine("ServerView n:" + view.Count);
            Task[] tasks = new Task[view.Count];
            int j = 0;
            foreach ( ServerService server in view)
            {
                Console.WriteLine("oooioi");
            }
            foreach (ServerService server in view)
            {
                if (server != this)
                {
                     Console.WriteLine("aqui");
                    Task t = Task.Run(() => server.CompareTerm(term,this));
                    tasks[j] = t;
                    j++;

                    Console.WriteLine("enviou para servidor " + server.getID());
                    //server.CompareTerm(term, id);
                    Console.WriteLine("i= " + i);
                    lock (_lock1)
                    {
                        Monitor.Wait(_lock1);
                    }
                   
                    if (i > (view.Count / 2))
                    {

                        // if (state == Estado.CANDIDATE)
                        //{
                        Console.WriteLine("Depois");
                        state = Estado.LEADER;
                        Console.WriteLine("Promovido a lider");
                        electionTimer.Stop();
                        heartbeatTimer = new System.Timers.Timer(1000);
                        heartbeatTimer.Elapsed += HeartbeatSend;
                        heartbeatTimer.AutoReset = true;
                        heartbeatTimer.Enabled = true;
                        Console.WriteLine("DONE");
                        return;
                        //  }

                    }
                }
               // Task.WaitAll(tasks);
            }
           //Task.WaitAll(tasks, 1000);
           // foreach (ServerService serv in view)
           // {
           //     Console.WriteLine("kakakakak"+serv.getID());
           //     if (!serv.getID().Equals(id))
           //     {
           //         Console.WriteLine("COMPARE "+ serv.getID());
           //         i += serv.CompareTerm(term);
           //         Console.WriteLine("---->"+i);
           //     }
           // }
            Console.WriteLine("ANTES: " + i+ "needed" + view.Count/2);
           
        }

        public void CompareTerm(int _term,ServerService server)
        {
            
            Console.WriteLine("COmpara o recebido de servidor "+server.getID()+" :" + _term + "com o meu:" + term);
            if (_term> term)
            {
                Console.WriteLine("e maior chefe");
                term = _term;
                server.CompareANS();
                electionTimer.Stop();
                electionTimer.Start();

               
                return;
            }
            else
            {
                Console.WriteLine("Nao e maior chefe");
                return  ;
            }
        }
        public void CompareANS()
        {
            //INCREMENTA O i LOCAL
            Console.WriteLine("TOMAAAA PUTA! i= "+i);
            i++;
            Console.WriteLine("i== " + i);
            lock (_lock1)
            {
                Monitor.PulseAll(_lock1);
            }

        }

        private void HeartbeatSend(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("HBSend");
            foreach (ServerService serv in view)
            {
                if (!serv.getID().Equals( id))
                {
                    Console.WriteLine("HeartBeat Sent");
                    serv.Heartbeat();
                }
            }
        }
        public void Heartbeat()
        {
            Console.WriteLine("Ping");
            state = Estado.FOLLOWER;
            electionTimer.Stop();
            electionTimer.Start();
        } 

        public void removeId(string id)
        {
            foreach(ServerService serv in view)
            {
                if (serv.getID().Equals(id)){
                    Console.WriteLine("VAIS SER APAGADO CRLHES" + serv.getID());
                    view.Remove(serv);
                }
                Console.WriteLine("FICASTE CRLH "+serv.getID());
            }
        }

        public void Logout()
        {
            foreach(ServerService serv in view)
            {
                serv.removeId(this.id);
            }
        }
    }
}
