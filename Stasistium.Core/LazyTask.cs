using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Stasistium.Core
{
    public class LazyTask
    {
        private readonly Func<Task> action;
        private int set;
        private readonly TaskCompletionSource<Task> completionSource = new TaskCompletionSource<Task>();

        //
        // Summary:
        //     Gets an awaiter used to await this System.Threading.Tasks.Task.
        //
        // Returns:
        //     An awaiter instance.
        public TaskAwaiter GetAwaiter()
        {
            var localTask = this.AsTask();
            return localTask.GetAwaiter();
        }

        public Task AsTask()
        {
            var old = System.Threading.Interlocked.CompareExchange(ref this.set, 1, 0);
            if (old == 0)
                this.completionSource.SetResult(this.action());
            return this.completionSource.Task.Unwrap();
        }


        public LazyTask(Func<Task> action)
        {
            this.action = action ?? throw new ArgumentNullException(nameof(action));
        }
        public LazyTask(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            this.action = () => { action(); return Task.CompletedTask; };
        }

        public static LazyTask<T> Create<T>(Func<Task<T>> action) => new LazyTask<T>(action);
        public static LazyTask<T> Create<T>(Func<T> action) => new LazyTask<T>(action);
        public static LazyTask Create(Func<Task> action) => new LazyTask(action);
        public static LazyTask Create(Action action) => new LazyTask(action);
    }

}
