using System;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    public class ContinueWithOnCanceledTaskTests
    {
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(2);

        [Fact]
        public void RunToCompletionContinuationOptionWhileFirstIsCanceled()
        {
            /* Arrange */
            Task<int> canceled = Factory.CreateCanceledTask();

            /* Act */
            Task<int> second = canceled
                .ContinueWith(t => t.Result + 5, TaskContinuationOptions.OnlyOnRanToCompletion);

            /* Assert */
            AdditionalAssertions.WaitIsCanceled(second);
            Assert.Null(second.Exception);
        }     

        [Fact]
        public void NextTaskInChainWhilePreviousIsCanceled()
        {
            /* Arrange */
            Task<int> canceled = Factory.CreateCanceledTask();

            /* Act */
            Task<int> result = canceled
                .ContinueWith(t => t.Result + 5);

            /* Assert */
            result.WaitSafe();
            Assert.Throws<AggregateException>(() => result.Wait(timeout));
            TaskCanceledException exception = (TaskCanceledException) result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);

            // status
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.True(result.IsFaulted);
            Assert.False(result.IsCanceled);
        }

        [Fact]
        public void NextTaskInChainWhilePreviousIsFaulted()
        {
            /* Arrange */
            Task<int> canceled = Task.FromException<int>(new ApplicationException());

            /* Act */
            Task<int> result = canceled
                .ContinueWith(t => t.Result + 5);

            /* Assert */
            result.WaitSafe();
            Assert.Throws<AggregateException>(() => result.Wait(timeout));
            TaskCanceledException exception = (TaskCanceledException) result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);

            // status
            Assert.Equal(TaskStatus.Faulted, result.Status);
            Assert.True(result.IsFaulted);
            Assert.False(result.IsCanceled);
        }
    }
}
