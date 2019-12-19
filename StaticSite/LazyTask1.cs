using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StaticSite
{
    public class LazyTask<T>
    {
        private readonly Func<Task<T>> action;
        private readonly TaskCompletionSource<Task<T>> completionSource = new TaskCompletionSource<Task<T>>();
        private int set;

        //
        // Summary:
        //     Gets an awaiter used to await this System.Threading.Tasks.Task.
        //
        // Returns:
        //     An awaiter instance.
        public TaskAwaiter<T> GetAwaiter()
        {
            var localTask = this.AsTask();
            return localTask.GetAwaiter();

        }

        public Task<T> AsTask()
        {
            var old = System.Threading.Interlocked.CompareExchange(ref this.set, 1, 0);
            if (old == 0)
                this.completionSource.SetResult(this.action());
            return this.completionSource.Task.Unwrap();
        }

        public LazyTask(Func<Task<T>> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }
        public LazyTask(Func<T> action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            this.action = () =>
            {
                try
                {
                    return Task.FromResult(action());
                }
                catch (TaskCanceledException e)
                {
                    return Task.FromCanceled<T>(e.CancellationToken);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    return Task.FromException<T>(e);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            };
        }
    }

}
