using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetMaster
{
    class PuppetMasterServices
    {
        private List<PcsService> Pcs;

        public PuppetMasterServices(string confFile)
        {
            Pcs = new List<PcsService>();

            using (StreamReader file = File.OpenText("../../../clientConf.txt"))
            {
                string line;       
                while ((line = file.ReadLine()) != null)
                {
                    PcsService obj = (PcsService)Activator.GetObject(
                    typeof(PcsService),
                    line);

                    Pcs.Add(obj);
                }                  
            }            
        }

        public void StartServer(string serverIDd, string Url, int minDelay, int maxDelay)
        {

        }

        public void StartClient(string serverIDd, string Url, string scriptFile)
        {

        }

        public void PrintStatus()
        {

        }
    }
}
