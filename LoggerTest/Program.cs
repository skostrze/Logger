using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LoggerTest
{
    class Program
    {
        /**
         * Test Example
         * 
         **/
        static void Main(string[] args)
        {

            

            Logger.Logger.Instance.setLogger(1, Environment.CurrentDirectory + "\\logs\\", "name", 100000);



            for (int i = 0; i < 1000; i++)
            {
                Logger.Logger.Instance.WriteToLog("main", "test: " + i, Logger.LogMessagePriority.Info);
                System.Console.WriteLine("test:" + i);
                System.Threading.Thread.Sleep(1);
            }
            System.Console.Write(Environment.CurrentDirectory + "\\logs\\");
            System.Console.ReadKey();
        }
    }
}
