using System.Threading;
using System.Threading.Tasks;

namespace TaskExamples
{
    public class Factory
    {
        public static Task<int> CreateCanceledTask()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.SetCanceled();
            return tcs.Task;

            // the other option for .NET 4.6 is
            var cts = new CancellationTokenSource();
            cts.Cancel();
            return Task.FromCanceled<int>(cts.Token);
        }
    }
}