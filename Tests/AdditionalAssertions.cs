using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskExamples.Tests
{
    internal static class AdditionalAssertions
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(2);
        
        public static void IsCanceled(Task task)
        {
            Assert.Throws<AggregateException>(() => task.Wait(timeout));
            Assert.AreEqual(TaskStatus.Canceled, task.Status);
        }
    }
}
