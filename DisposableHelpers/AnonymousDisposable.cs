using System;
using System.Threading.Tasks;

namespace DisposableHelpers
{
    /// <summary>
    /// Provides disposable action with <see cref="IDisposable"/> implementation.
    /// </summary>
    public class AnonymousDisposable :
        Disposable
    {
        private readonly Action? dispose;
#if NETSTANDARD2_1_OR_GREATER
        private readonly Task? disposeAsync;
#endif

        /// <summary>
        /// Creates an instance of <see cref="AnonymousDisposable"/> class.
        /// </summary>
        public AnonymousDisposable(Action dispose)
        {
            this.dispose = dispose;
        }

#if NETSTANDARD2_1_OR_GREATER
        /// <summary>
        /// Creates an instance of <see cref="AnonymousDisposable"/> class.
        /// </summary>
        public AnonymousDisposable(Task disposeAsync)
        {
            this.disposeAsync = disposeAsync;
        }
#endif

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dispose?.Invoke();
                if (disposeAsync != null)
                {
                    disposeAsync?.RunSynchronously();
                }
            }
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                dispose?.Invoke();
                if (disposeAsync != null)
                {
                    await disposeAsync;
                }
            }
            await base.DisposeAsync(disposing);
        }
    }
}
