using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples
{
    internal static class AdditionalAssertions
    {
        public static void WaitIsCanceled(Task task)
        {
            task.WaitSafe();
            Assert.Equal(TaskStatus.Canceled, task.Status);
        }

        public static void WaitIsFaulted(Task task)
        {
            task.WaitSafe();
            Assert.Equal(TaskStatus.Faulted, task.Status);
        }

        public static void ThrowsTaskCanceledException(Func<object> action)
        {
            AggregateException aggregateException = Assert.Throws<AggregateException>(action);
            IsTaskCanceledAggregateException(aggregateException);
        }

        public static void IsTaskCanceledAggregateException(Exception aggregateException)
        {
            AggregateException castedAggregateException = Assert.IsType<AggregateException>(aggregateException);

            Exception exception = castedAggregateException.InnerExceptions.Single();
            Assert.IsType<TaskCanceledException>(exception);
            Assert.Null(exception.InnerException);
        }
    }
}
