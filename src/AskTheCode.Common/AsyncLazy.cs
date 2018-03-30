using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// Provides support for asynchronous lazy initialization. This type is fully threadsafe.
    /// </summary>
    /// <remarks>
    /// Created by Stephen Cleary in the following blog post:
    /// http://blog.stephencleary.com/2012/08/asynchronous-lazy-initialization.html
    /// </remarks>
    /// <typeparam name="T">The type of object that is being asynchronously initialized.</typeparam>
    public sealed class AsyncLazy<T>
    {
        /// <summary>
        /// The underlying lazy task.
        /// </summary>
        private readonly Lazy<Task<T>> instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncLazy{T}"/> class.
        /// </summary>
        /// <param name="factory">The asynchronous delegate that is invoked on a background thread to produce the value when it is needed.</param>
        public AsyncLazy(Func<Task<T>> factory)
        {
            this.instance = new Lazy<Task<T>>(() => Task.Run(factory));
        }

        /// <summary>
        /// Asynchronous infrastructure support. This method permits instances of <see cref="AsyncLazy{T}"/> to be await'ed.
        /// </summary>
        public TaskAwaiter<T> GetAwaiter()
        {
            return this.instance.Value.GetAwaiter();
        }

        /// <summary>
        /// Starts the asynchronous initialization, if it has not already started.
        /// </summary>
        public void Start()
        {
            var unused = this.instance.Value;
        }
    }
}
