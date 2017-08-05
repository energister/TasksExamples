using System.Threading.Tasks;

namespace TaskExamples
{
    public static class TaskExtensions
    {
        public static void WaitSafe(this Task task)
        {
            task.ContinueWith(t => { }).Wait();
        }
    }
}
