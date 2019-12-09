using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace StaticSite
{
    public class LazyTask<T>
    {
        private readonly Func<Task<T>> action;
        private Task<T>? task;

        //
        // Summary:
        //     Gets an awaiter used to await this System.Threading.Tasks.Task.
        //
        // Returns:
        //     An awaiter instance.
        public TaskAwaiter<T> GetAwaiter()
        {
            var localTask = this.task ?? (this.task = this.action());
            return localTask.GetAwaiter();

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
