using System;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// Allows for running async methods synchronously
    /// </summary>
    public static class AsyncHelper
    {
        /// <summary>
        /// The task factory for creating tasks to be run on the threadpool
        /// </summary>
        private static readonly TaskFactory TaskFactory = new TaskFactory(
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskContinuationOptions.None,
            TaskScheduler.Default);

        /// <summary>
        /// Runs an async method synchronously and returns the result
        /// </summary>
        /// <typeparam name="TResult">Type of result</typeparam>
        /// <param name="asyncFunc">The async method to run</param>
        /// <returns>The result of the async method</returns>
        public static TResult RunSync<TResult>(Func<Task<TResult>> asyncFunc)
        {
            return TaskFactory
                .StartNew(asyncFunc)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Runs an async method synchronously
        /// </summary>
        /// <param name="asyncFunc">The async method to run</param>
        public static void RunSync(Func<Task> asyncFunc)
        {
            TaskFactory
                .StartNew(asyncFunc)
                .Unwrap()
                .GetAwaiter()
                .GetResult();
        }
    }
}