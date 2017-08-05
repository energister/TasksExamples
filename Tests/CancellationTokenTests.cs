using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    /// <summary>
    /// see Chaining Tasks by Using Continuation Tasks - Canceling a Continuation
    /// https://msdn.microsoft.com/en-us/library/ee372288(v=vs.110).aspx#Anchor_5
    /// and Task Cancellation
    /// https://msdn.microsoft.com/en-us/library/dd997396(v=vs.110).aspx
    /// </summary>
    public class CancellationTokenTests
    {
        private static int ThrowAfterSomeTimeIfCancellationRequested(CancellationToken cancellationToken)
        {
            string someString = string.Empty;
            for (int i = 0; ; i++)
            {
                someString += "a";
                if (i % 1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
            }

            return 5;
        }

        [Fact]
        public void ThrowOperationCanceledExceptionWithoutCancellationTokenSupplied()
        {
            /* Arrange */
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));

            /* Act */
            Task task = Task.Run(() => ThrowAfterSomeTimeIfCancellationRequested(source.Token));

            /* Assert */
            AdditionalAssertions.WaitIsFaulted(task);
            Assert.NotNull(task.Exception);
        }

        [Fact]
        public void ThrowOperationCanceledExceptionWithCancellationTokenSupplied()
        {
            /* Arrange */
            var source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));

            /* Act */
            Task<int> task = Task.Run(() => ThrowAfterSomeTimeIfCancellationRequested(source.Token), source.Token);

            /* Assert */
            AdditionalAssertions.WaitIsCanceled(task);
        }


        [Fact]
        public void SelfCanceledTask()
        {
            /* Arrange */
            CancellationTokenSource selfCts = new CancellationTokenSource();

            /* Act */
            Task<int> task = Task.Run(() =>
            {
                selfCts.Cancel();
                selfCts.Token.ThrowIfCancellationRequested();
                return 5;
            }, selfCts.Token);


            /* Assert */
            AdditionalAssertions.WaitIsCanceled(task);
        }

        [Fact]
        public void TaskCanceledBeforeStart()
        {
            /* Arrange */
            var source = new CancellationTokenSource();
            source.Cancel();

            // Act
            Task<int> task = Task.Run(() => 5, source.Token);

            // Assert
            AdditionalAssertions.WaitIsCanceled(task);
        }

        [Fact]
        public void CallbackIsCalledEvenIfTokenIsAlreadyCanceled()
        {
            /* Arrange */
            bool canceled = false;

            var source = new CancellationTokenSource();
            source.Cancel();

            /* Act */
            source.Token.Register(() => canceled = true);

            // Assert
            Assert.True(canceled);
        }

        [Fact]
        public void TokenIsAlreadyCanceledWhileCallbackIsCalled()
        {
            /* Arrange */
            bool canceled = false;

            var source = new CancellationTokenSource();

            source.Token.Register(() => { canceled = source.Token.IsCancellationRequested; });

            /* Act */
            source.Cancel();

            /* Assert */
            Assert.True(canceled);
        }
    }
}
