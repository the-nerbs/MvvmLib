using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MvvmLib.Tests
{
    /// <summary>
    /// Contains "tests" for experimenting with task behaviors. These aren't really tests per se,
    /// but are rather just sandboxes for testing what is and is not valid for interacting with
    /// Task objects.
    /// </summary>
    [TestClass]
    public class TaskTests
    {
        [TestMethod]
        public void TestContinueWithAfterCompletionAndMultipleContinuationsOnSameTask()
        {
            var t2 = Task
                .Run(() => throw new Exception("test"))
                .ContinueWith(t =>
                {
                    t.ContinueWith(t3 =>
                    {
                        Console.WriteLine("Inner ContinueWith");
                    });
                    Console.WriteLine("Outer ContinueWith");
                });

            t2.ContinueWith(t4 =>
            {
                Console.WriteLine("Second Outer ContinueWith");
            });
            Console.WriteLine("outside task");

            t2.Wait();
            Console.WriteLine("after wait");

            t2.ContinueWith(t5 =>
            {
                Console.WriteLine("ContinueWith after wait");
            }).Wait();
        }
    }
}
