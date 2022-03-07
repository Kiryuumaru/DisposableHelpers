using System;
using System.Threading;
using System.Threading.Tasks;

namespace DisposableHelpers
{
    /// <summary>
    /// Contains all methods for performing proper <see cref="IDisposable"/> operations.
    /// </summary>
    public class Disposable : object, IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this object is in the process of disposing.
        /// </summary>
        public bool IsDisposing => Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalStarted) == DisposalStarted;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref disposeStage, DisposalComplete, DisposalComplete) == DisposalComplete;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
        /// </summary>
        public bool IsDisposedOrDisposing => Interlocked.CompareExchange(ref disposeStage, DisposalNotStarted, DisposalNotStarted) != DisposalNotStarted;

        /// <summary>
        /// Gets the object name, for use in any <see cref="ObjectDisposedException"/> thrown by this object.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this property if they would like more control over the object name appearing in any <see cref="ObjectDisposedException"/>
        /// thrown by this <see cref="Disposable"/>. This can be particularly useful in debugging and diagnostic scenarios.
        /// </remarks>
        /// <value>
        /// The object name, which defaults to the class name.
        /// </value>
        protected virtual string? ObjectName => GetType().FullName;

        private const int DisposalNotStarted = 0;
        private const int DisposalStarted = 1;
        private const int DisposalComplete = 2;

        // see the constants defined above for valid values
        private int disposeStage;

#nullable enable
        private readonly Action<bool>? dispose;
#nullable disable

        #endregion

        #region Events

        /// <summary>
        /// Occurs when this object is about to be disposed.
        /// </summary>
#nullable enable
        public event EventHandler? Disposing;
#nullable disable

        #endregion

        #region Initializers

        /// <summary>
        /// Creates an instance of <see cref="Disposable"/> class.
        /// </summary>
        public Disposable()
        {

        }

        /// <summary>
        /// Creates an instance of <see cref="Disposable"/> class with action on dispose.
        /// </summary>
        public Disposable(Action<bool> dispose)
        {
            this.dispose = dispose;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Disposable"/> class.
        /// </summary>
        ~Disposable()
        {
            Dispose(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes of this object, if it hasn't already been disposed.
        /// </summary>
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
            {
                return;
            }

            OnDisposing();
            Disposing = null;

            dispose?.Invoke(true);
            Dispose(true);
            MarkAsDisposed();
        }

        /// <summary>
        /// Verifies that this object is not in the process of disposing, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposing()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object has not been disposed, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object is not being disposed or has been disposed, throwing an exception if either of these are true.
        /// </summary>
        protected void VerifyNotDisposedOrDisposing()
        {
            if (IsDisposedOrDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Allows subclasses to provide dispose logic.
        /// </summary>
        /// <param name="disposing">
        /// Whether the method is being called in response to disposal, or finalization.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
        }

        /// <summary>
        /// Raises the <see cref="Disposing"/> event.
        /// </summary>
        protected virtual void OnDisposing()
        {
            Disposing?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Marks this object as disposed without running any other dispose logic.
        /// </summary>
        /// <remarks>
        /// Use this method with caution. It is helpful when you have an object that can be disposed in multiple fashions, such as through a <c>CloseAsync</c> method.
        /// </remarks>
        protected void MarkAsDisposed()
        {
            GC.SuppressFinalize(this);
            _ = Interlocked.Exchange(ref disposeStage, DisposalComplete);
        }

        #endregion
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Contains all methods for performing proper <see cref="IAsyncDisposable"/> operations.
    /// </summary>
    public class AsyncDisposable : object, IAsyncDisposable
    {
        #region Properties

        /// <summary>
        /// Gets a value indicating whether this object is in the process of disposing.
        /// </summary>
        public bool IsDisposing => Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalStarted) == DisposalStarted;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        public bool IsDisposed => Interlocked.CompareExchange(ref disposeStage, DisposalComplete, DisposalComplete) == DisposalComplete;

        /// <summary>
        /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
        /// </summary>
        public bool IsDisposedOrDisposing => Interlocked.CompareExchange(ref disposeStage, DisposalNotStarted, DisposalNotStarted) != DisposalNotStarted;

        /// <summary>
        /// Gets the object name, for use in any <see cref="ObjectDisposedException"/> thrown by this object.
        /// </summary>
        /// <remarks>
        /// Subclasses can override this property if they would like more control over the object name appearing in any <see cref="ObjectDisposedException"/>
        /// thrown by this <see cref="Disposable"/>. This can be particularly useful in debugging and diagnostic scenarios.
        /// </remarks>
        /// <value>
        /// The object name, which defaults to the class name.
        /// </value>
        protected virtual string ObjectName => GetType().FullName;

        private const int DisposalNotStarted = 0;
        private const int DisposalStarted = 1;
        private const int DisposalComplete = 2;

        // see the constants defined above for valid values
        private int disposeStage;

#nullable enable
        private readonly Func<bool, ValueTask>? dispose;
#nullable disable

        #endregion

        #region Events

        /// <summary>
        /// Occurs when this object is about to be disposed.
        /// </summary>
#nullable enable
        public event EventHandler? Disposing;
#nullable disable

        #endregion

        #region Initializers

        /// <summary>
        /// Creates an instance of <see cref="AsyncDisposable"/> class.
        /// </summary>
        public AsyncDisposable()
        {

        }

        /// <summary>
        /// Creates an instance of <see cref="AsyncDisposable"/> class with action on dispose.
        /// </summary>
        public AsyncDisposable(Func<bool, ValueTask> dispose)
        {
            this.dispose = dispose;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="AsyncDisposable"/> class.
        /// </summary>
        ~AsyncDisposable()
        {
            DisposeAsync(false);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disposes of this object, if it hasn't already been disposed.
        /// </summary>
        /// <returns>
        /// The <see cref="ValueTask"/> of the dispose operation.
        /// </returns>
        public async ValueTask DisposeAsync()
        {
            if (Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
            {
                return;
            }

            OnDisposing();
            Disposing = null;

            if (dispose != null)
            {
                await dispose.Invoke(true);
            }
            await DisposeAsync(true);
            MarkAsDisposed();
        }

        /// <summary>
        /// Verifies that this object is not in the process of disposing, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposing()
        {
            if (IsDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object has not been disposed, throwing an exception if it is.
        /// </summary>
        protected void VerifyNotDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Verifies that this object is not being disposed or has been disposed, throwing an exception if either of these are true.
        /// </summary>
        protected void VerifyNotDisposedOrDisposing()
        {
            if (IsDisposedOrDisposing)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        /// <summary>
        /// Allows subclasses to provide dispose logic.
        /// </summary>
        /// <param name="disposing">
        /// Whether the method is being called in response to disposal, or finalization.
        /// </param>
        /// <returns>
        /// The <see cref="ValueTask"/> of the dispose operation.
        /// </returns>
        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            return default;
        }

        /// <summary>
        /// Raises the <see cref="Disposing"/> event.
        /// </summary>
        protected virtual void OnDisposing()
        {
            Disposing?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Marks this object as disposed without running any other dispose logic.
        /// </summary>
        /// <remarks>
        /// Use this method with caution. It is helpful when you have an object that can be disposed in multiple fashions, such as through a <c>CloseAsync</c> method.
        /// </remarks>
        protected void MarkAsDisposed()
        {
            GC.SuppressFinalize(this);
            _ = Interlocked.Exchange(ref disposeStage, DisposalComplete);
        }

        #endregion
    }
#endif
}
