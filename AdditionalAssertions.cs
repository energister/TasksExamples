using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples
{
    internal static class AdditionalAssertions
    {
        /// <summary>
        /// Every canceled task is conformed to this. No exceptions
        /// </summary>
        public static void WaitIsCanceled<T>(Task<T> task)
        {
            task.WaitSafe();
            Assert.Equal(TaskStatus.Canceled, task.Status);

            Assert.Null(task.Exception);

            AdditionalAssertions.ThrowsTaskCanceledException(() => task.Result);
            AdditionalAssertions.ThrowsTaskCanceledException(() => task.Wait(0));
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
