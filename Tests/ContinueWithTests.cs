using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    public class ContinueWithTests
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

            AggregateException resultException = Assert.Throws<AggregateException>(() => second.Result);
            AggregateException waitException = Assert.Throws<AggregateException>(() => second.Wait(0));

            AdditionalAssertions.IsTaskCanceledAggregateException(resultException);
            AdditionalAssertions.IsTaskCanceledAggregateException(waitException);
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
            AdditionalAssertions.WaitIsFaulted(result);

            AggregateException exception = result.Exception;
            AggregateException resultException = Assert.Throws<AggregateException>(() => result.Result);
            AggregateException waitException = Assert.Throws<AggregateException>(() => result.Wait(0));

            Assert.NotNull(exception);

            AdditionalAssertions.IsTaskCanceledAggregateException(exception.Flatten());
            AdditionalAssertions.IsTaskCanceledAggregateException(resultException.Flatten());
            AdditionalAssertions.IsTaskCanceledAggregateException(waitException.Flatten());

            AssertTaskCanceledAggregateExceptionInside(exception);
            AssertTaskCanceledAggregateExceptionInside(resultException);
            AssertTaskCanceledAggregateExceptionInside(waitException);
        }

        private void AssertTaskCanceledAggregateExceptionInside(AggregateException aggregateException)
        {
            // detailed structure
            Exception innerException = aggregateException.InnerExceptions.Single();
            AdditionalAssertions.IsTaskCanceledAggregateException(innerException);
        }

        [Fact]
        public void NextTaskInChainWhilePreviousIsFaulted()
        {
            /* Arrange */
            var applicationException = new ApplicationException();
            Task<int> faulted = Task.FromException<int>(applicationException);

            /* Act */
            Task<int> result = faulted
                .ContinueWith(t => t.Result + 5);

            /* Assert */
            AdditionalAssertions.WaitIsFaulted(result);

            Assert.NotNull(result.Exception);

            Assert.Throws<AggregateException>(() => result.Wait(timeout));
            TaskCanceledException exception = (TaskCanceledException) result.Exception.Flatten().InnerException;
        }
    }
}
