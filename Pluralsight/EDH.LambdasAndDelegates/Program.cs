﻿using System;

namespace EDEH.LambdasAndDelegates
{
    public delegate int BizRulesDelegate(int x, int y);

    public class Program
    {
        public static void Main(string[] args)
        {
            ProcessData processData = new ProcessData();

            //BizRulesDelegate addDelegate = (x, y) => x + y;
            //BizRulesDelegate multiplyDelegate = (x, y) => x * y;
            //processData.Process(2, 3, addDelegate);
            //processData.Process(2, 3, multiplyDelegate);

            //Action<int, int> addAction = (x, y) => Console.WriteLine(x + y);
            //Action<int, int> multiplyAction = (x, y) => Console.WriteLine(x * y);
            //processData.ProcessAction(2, 3, addAction);
            //processData.ProcessAction(2, 3, multiplyAction);

            Func<int, int, int> funcAddDel = (x, y) => x + y;
            Func<int, int, int> funcMutliplyDel = (x, y) => x * y;
            processData.ProcessFunc(2, 3, funcAddDel);
            processData.ProcessFunc(2, 3, funcMutliplyDel);

            //Worker worker = new Worker();
            //worker.WorkPerformed += (s, e) =>
            //{
            //    Console.WriteLine("Worked: " + e.Hours + " hour(s) doing: " + e.WorkType);
            //    Console.WriteLine("Some other value");
            //};
            //worker.WorkCompleted += (s, e) =>
            //{
            //    Console.WriteLine("Work is complete!");
            //};
            //worker.DoWork(8, WorkType.GenerateReports);

            Console.Read();
        }

        //public static void Worker_WorkPerformed(object sender, WorkPerformedEventArgs e)
        //{
        //    Console.WriteLine("Worked: " + e.Hours + " hour(s) doing: " + e.WorkType);
        //}

        //public static void Worker_WorkCompleted(object sender, EventArgs e)
        //{
        //    Console.WriteLine("Work is complete!");
        //}
    }
}