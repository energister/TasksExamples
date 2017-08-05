using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    /// <summary>
    /// The goal is having two tasks (Task&lt;int&gt; and Task&lt;string&gt;)
    /// got the task that will calculate it's result based on both previous results
    /// and will be canceled if one of the source tasks were canceled
    /// and will be failed if one of the source tasks were failed
    /// </summary>
    public class CalculateResultUponTwoTasksTests
    {
        private TimeSpan timout = TimeSpan.FromSeconds(2);
        
        //        // success or canceled
//        Task<int> aTask = Task.FromResult(0);
//
//        // suceess
//        Task<int> bTask = Task.FromResult(5);
//
        Task<string> alwaysFinished = Task.FromResult("OK");

        /* Option 1: Task.Factory.ContinueWhenAll */

        [Fact]
        public void BothSuccess()
        {
            Task<int> aTask = Task.FromResult(0);

            Task<string> result = Task.Factory.ContinueWhenAll(new Task[] { aTask, alwaysFinished }, tasks => Aggregate(aTask, alwaysFinished));

            Assert.True(result.Wait(timout));
        }

        [Fact]
        public void FirstFailed()
        {
            Task<int> aTask = Task.Run(new Func<int>(
                                           () => { throw new ApplicationException("Unable to calculate first value"); }));

            Task<string> result = Task.Factory.ContinueWhenAll(new Task[] { aTask, alwaysFinished }, tasks => Aggregate(aTask, alwaysFinished));

            Assert.Throws<AggregateException>(() => result.Wait(timout));
            ApplicationException exception = (ApplicationException) result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);
            Assert.True(exception.Message.Contains("calculate"));
        }

        [Fact]
        public void FirstCanceled()
        {
            Task<int> aTask = CreateCanceledTask();

            Task<string> result = Task.Factory.ContinueWhenAll(new Task[] { aTask, alwaysFinished }, tasks => Aggregate(aTask, alwaysFinished));

            Assert.Throws<AggregateException>(() => result.Wait(timout));
            TaskCanceledException exception = (TaskCanceledException)result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);
            Assert.Equal(TaskStatus.Faulted, result.Status);
        }

        [Fact]
        public void CancelResultWhenFirstIsCanceled()
        {
            CancellationTokenSource selfCts = new CancellationTokenSource();
            
            Task<int> aTask = CreateCanceledTask();

            Task<string> result = Task.Factory.ContinueWhenAll(new Task[] { aTask, alwaysFinished }, tasks =>
            {
                if (aTask.IsCanceled)
                {
                    selfCts.Cancel();
                    selfCts.Token.ThrowIfCancellationRequested();
                    
                    return null;
                }

                return aTask.Result + alwaysFinished.Result;
            }, selfCts.Token);

            AdditionalAssertions.WaitIsCanceled(result);
        }



        /* Option 2: Task.WhenAll - code is much smaller */

        private Task<string> Expression2(Task<int> firstTask, Task<string> alwaysFinished)
        {
            return Task.WhenAll(firstTask, alwaysFinished)
                .ContinueWith(_ => Aggregate(firstTask, alwaysFinished),
                              TaskContinuationOptions.NotOnCanceled);
        }

        [Fact]
        public void BothSuccessOption2()
        {
            Task<int> aTask = Task.FromResult(0);

            Task<string> result = Expression2(aTask, alwaysFinished);

            Assert.True(result.Wait(timout));
        }

        [Fact]
        public void FirstFailedOption2()
        {
            Task<int> aTask = Task.Run(new Func<int>(
                                           () => { throw new ApplicationException("Unable to calculate first value"); }));

            Task<string> result = Expression2(aTask, alwaysFinished);

            Assert.Throws<AggregateException>(() => result.Wait(timout));
            ApplicationException exception = (ApplicationException)result.Exception.Flatten().InnerException;
            Assert.NotNull(exception);
            Assert.True(exception.Message.Contains("calculate"));
        }
        
        [Fact]
        public void CancelResultWhenFirstIsCanceledOption2()
        {
            Task<int> aTask = CreateCanceledTask();

            Task<string> result = Expression2(aTask, alwaysFinished);

            AdditionalAssertions.WaitIsCanceled(result);
        }
        
        [Fact]
        public void UnableToSpecifyOnlyAndNotOptions()
        {
//            ArgumentOutOfRangeException ex = Assert.ThrowsAsync<>()<ArgumentOutOfRangeException>(() => Task.Factory.ContinueWhenAll(
//                new Task[] { }, tasks => { },
//                TaskContinuationOptions.OnlyOnCanceled));
//
//            Assert.Equal("It is invalid to exclude specific continuation kinds for continuations off of multiple tasks.\r\nParameter name: continuationOptions", ex.Message);
        }

        private string Aggregate(Task<int> a, Task<string> b)
        {
            return a.Result + b.Result;
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
