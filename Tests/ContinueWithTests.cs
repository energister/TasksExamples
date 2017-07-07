using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskExamples.Tests
{
    [TestFixture]
    public class ContinueWithTests
    {
        private readonly TimeSpan timeout = TimeSpan.FromSeconds(2);
        
        [Test]
        public void RunToCompletionWhileFirstIsCanceled()
        {
            /* Arrange */
            Task<int> firstTask = CreateCanceledTask();

            /* Act */
            Task<int> result = firstTask
                .ContinueWith(t => t.Result + 5, TaskContinuationOptions.OnlyOnRanToCompletion);

            /* Assert */
            AdditionalAssertions.IsCanceled(result);
        }

        [Test]
        public void SecondTaskWhileFirstIsCanceled()
        {
            /* Arrange */
            Task<int> firstTask = CreateCanceledTask();

            /* Act */
            Task<int> result = firstTask
                .ContinueWith(t => t.Result + 5);

            /* Assert */
            Assert.Throws<AggregateException>(() => result.Wait(timeout));
            TaskCanceledException exception = (TaskCanceledException) result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);
            Assert.AreEqual(TaskStatus.Faulted, result.Status);
        }

        private static Task<int> CreateCanceledTask()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            Task<int> aTask = tcs.Task;
            return aTask;
        }
    }
}
