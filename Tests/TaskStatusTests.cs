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
        }

        [Fact]
        public void Canceled()
        {
            /* Arrange */
            Task task = Factory.CreateCanceledTask();

            /* Assert */
            Assert.True(task.IsCompleted);
            Assert.True(task.IsCanceled);

            Assert.Equal(TaskStatus.Canceled, task.Status);
            Assert.Null(task.Exception);
        }
    }
}
