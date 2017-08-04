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
            Assert.True(task.IsCanceled);
        }
    }
}
