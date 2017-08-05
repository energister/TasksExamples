using System;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples
{
    internal static class AdditionalAssertions
    {
        private static readonly TimeSpan timeout = TimeSpan.FromSeconds(2);
        
        public static void WaitIsCanceled(Task task)
        {
            Assert.Throws<AggregateException>(() => task.Wait(timeout));
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        public static void WaitIsFaulted(Task task)
        {
            task.WaitSafe();
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }
    }
}
