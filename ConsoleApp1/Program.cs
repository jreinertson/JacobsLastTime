using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TimingExtensions
{
    class Program
    {
        static void Main(string[] args)
        {
            RunPollingService pollingService = new RunPollingService("http://localhost:80/runs", TimeSpan.FromMilliseconds(500));

            
            Timer timer = new Timer((e) =>
            {
                Console.Clear();
                Console.Write(pollingService.getCurrentRun());
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            

            Console.ReadLine();
        }
    }
}

