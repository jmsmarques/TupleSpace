﻿using System;
using System.Collections;
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
        static System.Threading.Timer TTimer;
        static ConsoleColor defaultC = Console.ForegroundColor;
        private static int i = 1;
        private static List<IServerService> view;
        private List<List<string>> tuples;
        private ArrayList holdBackQueue;
        private Queue deliverQueue;
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
        private static System.Timers.Timer requestTimer;
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
            state = Estado.FOLLOWER;

            Console.WriteLine("MEU ID:" + id);

            if(comType == 2)
            {
                holdBackQueue = ArrayList.Synchronized(new ArrayList());
                deliverQueue = Queue.Synchronized(new Queue());
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
            while (freeze) ;

            p = Math.Max(p , a) + 1;
            lock (_lock1)
            {
                AddToHoldQueue(id, p, req, tuple);
            }

            return p;
        }

        public List<string> XlConfirmation(string id, int nr)
        {            
            if(a != nr)
            {
                a = Math.Max(nr, a);
            }

            CheckChange(id, nr);

            while (true)
            {                                            
                if (((XlObj)holdBackQueue[0]).Agreed && nr == ((XlObj)holdBackQueue[0]).Nr)
                {
                    deliverQueue.Enqueue(holdBackQueue[0]);
                    holdBackQueue.Remove(holdBackQueue[0]);
                    break;
                }                
            }

            List<string> result = null;
            XlObj aux1 = (XlObj)deliverQueue.Peek();
            while (!id.Equals(aux1.Id)  || nr != aux1.Nr)
            {
                aux1 = (XlObj)deliverQueue.Peek();
            }

            List<string> aux;

            if ((((XlObj)deliverQueue.Peek()).Req).Equals("Add"))
            {
                aux = ((XlObj)deliverQueue.Dequeue()).Tuple;
                Add(aux);
            }
            else if ((((XlObj)deliverQueue.Peek()).Req).Equals("Read"))
            {
                aux = ((XlObj)deliverQueue.Dequeue()).Tuple;
                result = Read(aux);
            }
            else if ((((XlObj)deliverQueue.Peek()).Req).Equals("Take"))
            {
                aux = ((XlObj)deliverQueue.Dequeue()).Tuple;
                result = Take(aux);
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
                Console.WriteLine("[DEBUG] ADD is a XL");
                lock (_lock)
                    {
                        tuples.Add(tuple);
                        //test += 2;
                        //Console.WriteLine(test);
                }
                Console.WriteLine(frozen_req);
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
            /*Object[] queue = new Object[5];
            //Action a;
            queue[0] = req;
            queue[1] = nr;
            queue[2] = id;
            queue[3] = false;
            queue[4] = tuple;   */
            XlObj queue = new XlObj(req, nr, id, false, tuple);

            CheckPosition(nr, queue);            
        }

        private void CheckPosition(int nr, XlObj queue)
        {
            int aux = 0;
            
            foreach (XlObj obj in holdBackQueue)
            {
                if (obj.Nr < nr)
                {
                    holdBackQueue.Insert(aux, queue);
                    break;
                }
                else if(obj.Nr == nr)
                {
                    if(String.CompareOrdinal(obj.Id, queue.Id) > 0) //compares id for tiebreaker
                    {                    
                        holdBackQueue.Insert(aux, queue);
                        break;
                    }                    
                }
                aux++;
            }/*
            int i = 0;
            for(i = 0; i < holdBackQueue.Count; i++)
            {
                if ((int)holdBackQueue[i][1] < nr)
                {
                    holdBackQueue.Insert(i, queue);
                    break;
                }
                else if ((int)holdBackQueue[i][1] == nr)
                {
                    if (String.CompareOrdinal((string)holdBackQueue[0][2], (string)queue[2]) > 0) //compares id for tiebreaker
                    {
                        holdBackQueue.Insert(i, queue);
                        break;
                    }
                }
                //aux++;
            }*/

            if (aux == holdBackQueue.Count)
            {
                holdBackQueue.Add(queue);
            }
        }

        private void CheckChange(string id, int nr)
        {
            XlObj aux = null;
            foreach(XlObj obj in holdBackQueue)
            {
                if(obj.Id.Equals(id))
                {
                    obj.Agreed = true;
                    if (obj.Nr != nr)
                    {
                        aux = obj;                        
                        holdBackQueue.Remove(obj);
                    }                    
                    break;
                }
            }
            /*
            for(int i = 0; i < holdBackQueue.Count; i++)
            {
                if (((string)holdBackQueue[i][2]).Equals(id))
                {
                    holdBackQueue[i][3] = true;
                    if ((int)holdBackQueue[i][1] != nr)
                    {
                        aux = holdBackQueue[i];
                        holdBackQueue.Remove(holdBackQueue[i]);
                    }
                    break;
                }
            }*/
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
        public void SetReqTimer()
        {
            Console.WriteLine("TIMER RUNNING");
            requestTimer = new System.Timers.Timer(500);
            requestTimer.Elapsed += CancelRequest;
            requestTimer.AutoReset = true;
            requestTimer.Enabled = true;
        }
        private void CancelRequest(Object source, ElapsedEventArgs e)
        {
            Console.WriteLine("CANCEL REQUEST");
            requestTimer.Stop();
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
            if (view.Count == 1)
            {
                Console.WriteLine("Depois");
                state = Estado.LEADER;
                Console.WriteLine("Promovido a lider");
                electionTimer.Stop();
                heartbeatTimer = new System.Timers.Timer(500);
                heartbeatTimer.Elapsed += HeartbeatSend;
                heartbeatTimer.AutoReset = true;
                heartbeatTimer.Enabled = true;
                Console.WriteLine("DONE");
                return;
            }
            else {
                foreach (ServerService server in view)
                {
                    if (server != this)
                    {
                        //SetReqTimer();
                        Console.WriteLine("tentou para servidor posicao " + j);
                        Task t = Task.Run(() => server.CompareTerm(term, this));
                        tasks[j] = t;
                        j++;

                        // Console.WriteLine("enviou para servidor " + server.getID());
                        //server.CompareTerm(term, id);
                        Console.WriteLine("-------i= " + i);
                        //requestTimer.Stop();
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

                }
                 //Task.WaitAll(tasks, 2000);
                Console.WriteLine("DESBLOQUEOUS");
            }
            Console.WriteLine("aaaaaaaaaaa-------i= " + i);
           /* if (i > (view.Count / 2))
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
            */
            
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
                state = Estado.FOLLOWER;
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
            //VerificaLeader();
            lock (_lock1)
             {
              Monitor.PulseAll(_lock1);
           }

        }
        public void VerificaLeader()
        {
            if (i > (view.Count / 2))
            {

                if (state == Estado.CANDIDATE)
                {
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
                }

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

        public void RemoveId(ServerService server)
        {
            view.Remove(server);
        }

        public void Logout()
        {            
            for(int i = 0; i < view.Count; i++)
                if (!((ServerService)view[i]).Equals(this))
                {
                    ((ServerService)view[i]).RemoveId(this);
                }                
        }
    }
}
