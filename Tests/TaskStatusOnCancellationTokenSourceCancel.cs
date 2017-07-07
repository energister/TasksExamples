using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace TaskExamples.Tests
{
    /// <summary>
    /// see Chaining Tasks by Using Continuation Tasks - Canceling a Continuation
    /// https://msdn.microsoft.com/en-us/library/ee372288(v=vs.110).aspx#Anchor_5
    /// and Task Cancellation
    /// https://msdn.microsoft.com/en-us/library/dd997396(v=vs.110).aspx
    /// </summary>
    [TestFixture]
    public class TaskStatusOnCancellationTokenSourceCancel
    {
        [Test]
        public void ContinuationTaskIsFailed()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));
            Task<int> task = Task.Run(() => slowFunc(1, 2, source.Token));

            Thread.Sleep(2000);
            // (A canceled task will raise an exception when awaited).
            
            Assert.AreEqual(TaskStatus.Faulted, task.Status);
            Assert.NotNull(task.Exception);
        }

        [Test]
        public void ContinuationTaskIsCanceled()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(1));
            Task<int> task = Task.Run(() => slowFunc(1, 2, source.Token), source.Token);

            AdditionalAssertions.IsCanceled(task);
        }


        [Test]
        public void SelfCanceledTask()
        {
            CancellationTokenSource selfCts = new CancellationTokenSource();
            Task task = Task.Run(() =>
            {
                selfCts.Cancel();
                selfCts.Token.ThrowIfCancellationRequested();
            }, selfCts.Token);

            AdditionalAssertions.IsCanceled(task);
        }

        [Test]
        public void TaskCanceledBeforeStart()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            source.Cancel();

            // Act
            Task task = Task.Run(() => { }, source.Token);

            // Assert
            AdditionalAssertions.IsCanceled(task);
        }

        [Test]
        public void TokenIsAlreadyCanceledOnRegisterCallbackCall()
        {
            TaskCompletionSource<int> tcs = new TaskCompletionSource<int>();
            
            CancellationTokenSource source = new CancellationTokenSource();
            source.Token.Register(() => tcs.SetResult(5));

            Task task = tcs.Task.ContinueWith(_ => { }, source.Token);

            // Act
            source.Cancel();

            // Assert
            AdditionalAssertions.IsCanceled(task);
        }

        private int slowFunc(int a, int b, CancellationToken cancellationToken)
        {
            string someString = string.Empty;
            for (int i = 0; i < 200000; i++)
            {
                someString += "a";
                if (i%1000 == 0)
                    cancellationToken.ThrowIfCancellationRequested();
            }

            return a + b;
        }
    }
}
