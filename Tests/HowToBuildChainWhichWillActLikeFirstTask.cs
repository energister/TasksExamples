using System.Threading.Tasks;
using Xunit;

namespace TaskExamples.Tests
{
    public class HowToBuildChainWhichWillActLikeFirstTask
    {
        private static Task<int> AddTaskToChain(Task<int> previous)
        {
            return previous
                .ContinueWith(t => t.Result + 5, TaskContinuationOptions.NotOnCanceled);
        }

        [Fact]
        public void SuccessCase()
        {
            /* Arrange */
            Task<int> successed = Task.FromResult(1);

            /* Act */
            Task<int> second = AddTaskToChain(successed);

            /* Assert */
            var gotResult = second.Result;
            Assert.Equal(TaskStatus.RanToCompletion, second.Status);
        }

        [Fact]
        public void CanceledCase()
        {
            /* Arrange */
            Task<int> canceled = Factory.CreateCanceledTask();

            /* Act */
            Task<int> second = AddTaskToChain(canceled);

            /* Assert */
            AdditionalAssertions.WaitIsCanceled(second);
        }

        [Fact]
        public void FailedCase()
        {
            /* Arrange */
            Task<int> faulted = Factory.CreateFaultedTask();

            /* Act */
            Task<int> second = AddTaskToChain(faulted);

            /* Assert */
            AdditionalAssertions.WaitIsFaulted(second);
        }
    }
}