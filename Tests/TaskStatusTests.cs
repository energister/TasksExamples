using System;
using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    public class TaskStatusTests
    {
        [Fact]
        public void FinisedSuccessfully()
        {
            /* Arrange */
            Task task = Task.CompletedTask;

            /* Assert */
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.RanToCompletion, task.Status);

            Assert.Null(task.Exception);

            Assert.True(task.Wait(0));
        }

        [Fact]
        public void Canceled()
        {
            /* Arrange */
            Task<int> task = Factory.CreateCanceledTask();

            /* Assert */
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.Canceled, task.Status);
            Assert.True(task.IsCanceled);

            Assert.Null(task.Exception);

            Assert.Throws<AggregateException>(() => task.Result);
            Assert.Throws<AggregateException>(() => task.Wait());
        }

        [Fact]
        public void Faulted()
        {
            /* Arrange */
            Task<int> task = Task.FromException<int>(new ApplicationException());

            /* Assert */
            Assert.True(task.IsCompleted);
            Assert.Equal(TaskStatus.Faulted, task.Status);
            Assert.True(task.IsFaulted);

            Assert.NotNull(task.Exception);

            Assert.Throws<AggregateException>(() => task.Result);
            Assert.Throws<AggregateException>(() => task.Wait());
        }
    }
}
