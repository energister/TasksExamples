using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskExamples.Tests
{
    [TestFixture]
    public class ContinueWithOnCanceledTaskTests
    {
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(2);
        
        [Test]
        public void RunToCompletionWhileFirstIsCanceled()
        {
            /* Arrange */
            Task<int> firstTask = TasksHelper.CreateCanceledTask();

            /* Act */
            Task<int> result = firstTask
                .ContinueWith(t => t.Result + 5, TaskContinuationOptions.OnlyOnRanToCompletion);

            /* Assert */
            AdditionalAssertions.IsCanceled(result);
        }

        [Test]
        public void NextTaskInChainWhilePreviousIsCanceled()
        {
            /* Arrange */
            Task<int> firstTask = TasksHelper.CreateCanceledTask();

            /* Act */
            Task<int> result = firstTask
                .ContinueWith(t => t.Result + 5);

            /* Assert */
            Assert.Throws<AggregateException>(() => result.Wait(timeout));
            TaskCanceledException exception = (TaskCanceledException) result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);

            // status
            Assert.AreEqual(TaskStatus.Faulted, result.Status);
            Assert.True(result.IsFaulted);
            Assert.False(result.IsCanceled);
        }
    }
}
