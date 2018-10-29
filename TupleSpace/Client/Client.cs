using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write(".");
            System.Threading.Thread.Sleep(1000);
            Console.Write(".");
            System.Threading.Thread.Sleep(1000);
            Console.Write(".");
            System.Threading.Thread.Sleep(1000);
            Console.WriteLine();
            Console.WriteLine("Client Console\n\n");
            string input;
            while (true){
                Console.Write("PUPPET>");
                input= Console.ReadLine();
                string[] words = input.Split(' ');
                switch (words[0])
                {
                    case "add":
                        Console.WriteLine("add");
                        break;
                    case "read":
                        Console.WriteLine("read");
                        break;
                    case "take":
                        Console.WriteLine("take");
                        break;
                    case "wait":
                        Console.WriteLine("wait "+words[1]+"ms");
                        break;
                    case "begin-repeat":
                        Console.WriteLine("begin-repeat "+ words[1]+" times ");
                        break;
                     case "end-repeat":
                        Console.WriteLine("end-repeat");
                        break;
                    case "exit":
                    case"Exit":
                        Environment.Exit(1);
                        break;
                    default:
                        Console.WriteLine("Command not recognized. Type \"help\" for more info ");
                        break;
        }
    }
}
