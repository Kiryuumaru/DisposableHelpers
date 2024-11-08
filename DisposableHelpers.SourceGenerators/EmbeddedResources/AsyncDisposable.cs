/// <summary>
/// Contains all methods for performing proper <see cref="global::System.IAsyncDisposable"/> operations.
/// </summary>
public class AsyncDisposable : global::System.IAsyncDisposable
{
    /// <summary>
    /// Gets a value indicating whether this object is in the process of disposing.
    /// </summary>
    public bool IsDisposing => global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalStarted) == DisposalStarted;

    /// <summary>
    /// Gets a value indicating whether this object has been disposed.
    /// </summary>
    public bool IsDisposed => global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalComplete, DisposalComplete) == DisposalComplete;

    /// <summary>
    /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
    /// </summary>
    public bool IsDisposedOrDisposing => global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalNotStarted, DisposalNotStarted) != DisposalNotStarted;

    /// <summary>
    /// Gets the object name, for use in any <see cref="global::System.ObjectDisposedException"/> thrown by this object.
    /// </summary>
    /// <remarks>
    /// Subclasses can override this property if they would like more control over the object name appearing in any <see cref="global::System.ObjectDisposedException"/>
    /// thrown by this <see cref="Disposable"/>. This can be particularly useful in debugging and diagnostic scenarios.
    /// </remarks>
    /// <value>
    /// The object name, which defaults to the class name.
    /// </value>
#nullable enable
    protected virtual string? ObjectName => GetType().FullName;
#nullable disable

    private const int DisposalNotStarted = 0;
    private const int DisposalStarted = 1;
    private const int DisposalComplete = 2;

    // see the constants defined above for valid values
    private int disposeStage;

    /// <summary>
    /// Occurs when this object is about to be disposed.
    /// </summary>
#nullable enable
    public event global::System.EventHandler? Disposing;
#nullable disable

    private readonly global::System.Collections.Generic.List<global::System.Threading.CancellationTokenSource> cancelOnDisposeSources = new global::System.Collections.Generic.List<global::System.Threading.CancellationTokenSource>();
    private readonly global::System.Collections.Generic.List<global::System.Threading.CancellationTokenSource> cancelOnDisposingSources = new global::System.Collections.Generic.List<global::System.Threading.CancellationTokenSource>();

    /// <summary>
    /// Finalizes an instance of the <see cref="AsyncDisposable"/> class.
    /// </summary>
    ~AsyncDisposable()
    {
        DisposeAsync(false);
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    /// <returns>
    /// The <see cref="global::System.Threading.Tasks.ValueTask"/> of the dispose operation.
    /// </returns>
    public async System.Threading.Tasks.ValueTask DisposeAsync()
    {
        if (global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
        {
            return;
        }

        OnDisposing();
        Disposing = null;

        foreach (var cts in cancelOnDisposingSources)
        {
            cts.Cancel();
        }

        await DisposeAsync(true);

        global::System.GC.SuppressFinalize(this);
        global::System.Threading.Interlocked.Exchange(ref disposeStage, DisposalComplete);

        foreach (var cts in cancelOnDisposeSources)
        {
            cts.Cancel();
        }
    }

    /// <summary>
    /// Registers a <see cref="CancellationTokenSource"/> to be canceled when the object is fully disposed.
    /// </summary>
    /// <param name="cancellationTokenSource">The <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled.</param>
    public void CancelWhenDisposed(global::System.Threading.CancellationTokenSource cancellationTokenSource)
    {
        if (cancellationTokenSource == null)
            throw new global::System.ArgumentNullException(nameof(cancellationTokenSource));

        if (IsDisposed)
        {
            cancellationTokenSource.Cancel();
        }
        else
        {
            cancelOnDisposeSources.Add(cancellationTokenSource);
        }
    }

    /// <summary>
    /// Registers a <see cref="CancellationTokenSource"/> to be canceled when the object starts disposing.
    /// </summary>
    /// <param name="cancellationTokenSource">The <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled.</param>
    public void CancelWhenDisposing(global::System.Threading.CancellationTokenSource cancellationTokenSource)
    {
        if (cancellationTokenSource == null)
            throw new global::System.ArgumentNullException(nameof(cancellationTokenSource));

        if (IsDisposedOrDisposing)
        {
            cancellationTokenSource.Cancel();
        }
        else
        {
            cancelOnDisposingSources.Add(cancellationTokenSource);
        }
    }

    /// <summary>
    /// Registers a <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled when the object is fully disposed and returns its <see cref="global::System.Threading.CancellationToken"/>.
    /// </summary>
    /// <returns>A <see cref="CancellationToken"/> that will be canceled when the object is disposed.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposed()
    {
        var cancellationTokenSource = new global::System.Threading.CancellationTokenSource();
        if (IsDisposed)
        {
            cancellationTokenSource.Cancel();
        }
        else
        {
            cancelOnDisposeSources.Add(cancellationTokenSource);
        }
        return cancellationTokenSource.Token;
    }

    /// <summary>
    /// Registers a <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled when the object starts disposing and returns its <see cref="global::System.Threading.CancellationToken"/>.
    /// </summary>
    /// <returns>A <see cref="CancellationToken"/> that will be canceled when the object starts disposing.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposing()
    {
        var cancellationTokenSource = new global::System.Threading.CancellationTokenSource();
        if (IsDisposedOrDisposing)
        {
            cancellationTokenSource.Cancel();
        }
        else
        {
            cancelOnDisposingSources.Add(cancellationTokenSource);
        }
        return cancellationTokenSource.Token;
    }

    /// <summary>
    /// Verifies that this object is not in the process of disposing, throwing an exception if it is.
    /// </summary>
    protected void VerifyNotDisposing()
    {
        if (IsDisposing)
        {
            throw new global::System.ObjectDisposedException(ObjectName);
        }
    }

    /// <summary>
    /// Verifies that this object has not been disposed, throwing an exception if it is.
    /// </summary>
    protected void VerifyNotDisposed()
    {
        if (IsDisposed)
        {
            throw new global::System.ObjectDisposedException(ObjectName);
        }
    }

    /// <summary>
    /// Verifies that this object is not being disposed or has been disposed, throwing an exception if either of these are true.
    /// </summary>
    protected void VerifyNotDisposedOrDisposing()
    {
        if (IsDisposedOrDisposing)
        {
            throw new global::System.ObjectDisposedException(ObjectName);
        }
    }

    /// <summary>
    /// Raises the <see cref="Disposing"/> event.
    /// </summary>
    protected virtual void OnDisposing()
    {
        Disposing?.Invoke(this, new global::System.EventArgs());
    }

    /// <summary>
    /// Allows subclasses to provide dispose logic.
    /// </summary>
    /// <param name="disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    /// <returns>
    /// The <see cref="global::System.Threading.Tasks.ValueTask"/> of the dispose operation.
    /// </returns>
    protected virtual System.Threading.Tasks.ValueTask DisposeAsync(bool disposing)
    {
        return default;
    }
}
