/// <summary>
/// Contains all methods for performing proper <see cref="global::System.IDisposable"/> and <see cref="global::System.IAsyncDisposable"/> operations.
/// </summary>
public class Disposable : global::System.IDisposable, global::System.IAsyncDisposable
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

    private readonly global::System.Threading.CancellationTokenSource cancelOnDisposingCts = new global::System.Threading.CancellationTokenSource();
    private readonly global::System.Threading.CancellationTokenSource cancelOnDisposedCts = new global::System.Threading.CancellationTokenSource();

    /// <summary>
    /// Finalizes an instance of the <see cref="Disposable"/> class.
    /// </summary>
    ~Disposable()
    {
        Dispose(false);
    }

    private void CoreDispose()
    {
        if (global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
        {
            return;
        }

        OnDisposing();
        Disposing = null;

        cancelOnDisposingCts.Cancel();

        Dispose(true);
        DisposeAsync(true).AsTask().Wait();

        global::System.GC.SuppressFinalize(this);
        global::System.Threading.Interlocked.Exchange(ref disposeStage, DisposalComplete);

        cancelOnDisposedCts.Cancel();
    }

    private async global::System.Threading.Tasks.ValueTask CoreDisposeAsync()
    {
        if (global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
        {
            return;
        }

        OnDisposing();
        Disposing = null;

        cancelOnDisposingCts.Cancel();

        Dispose(true);
        await DisposeAsync(true);

        global::System.GC.SuppressFinalize(this);
        global::System.Threading.Interlocked.Exchange(ref disposeStage, DisposalComplete);

        cancelOnDisposedCts.Cancel();
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    public void Dispose()
    {
        CoreDispose();
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    /// <returns>
    /// The <see cref="global::System.Threading.Tasks.ValueTask"/> of the dispose operation.
    /// </returns>
    public global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        return CoreDisposeAsync();
    }

    /// <summary>
    /// Returns a <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is fully disposed, or when any of the provided <paramref name="cancellationTokens"/> is canceled.
    /// </summary>
    /// <param name="cancellationTokens">The <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled.</param>
    /// <returns>A <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is disposed.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposed(params global::System.Threading.CancellationToken[] cancellationTokens)
    {
        global::System.Collections.Generic.List<global::System.Threading.CancellationToken> ct = new global::System.Collections.Generic.List<global::System.Threading.CancellationToken>();
        ct.Add(cancelOnDisposedCts.Token);
        ct.AddRange(cancellationTokens);

        var cts = global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct.ToArray());

        if (IsDisposed)
        {
            cts.Cancel();
        }

        return cts.Token;
    }

    /// <summary>
    /// Returns a <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object starts disposing, or when any of the provided <paramref name="cancellationTokens"/> is canceled.
    /// </summary>
    /// <param name="cancellationTokens">The <see cref="global::System.Threading.CancellationTokenSource"/> to be canceled.</param>
    /// <returns>A <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is disposing.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposing(params global::System.Threading.CancellationToken[] cancellationTokens)
    {
        global::System.Collections.Generic.List<global::System.Threading.CancellationToken> ct = new global::System.Collections.Generic.List<global::System.Threading.CancellationToken>();
        ct.Add(cancelOnDisposingCts.Token);
        ct.AddRange(cancellationTokens);

        var cts = global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct.ToArray());

        if (IsDisposedOrDisposing)
        {
            cts.Cancel();
        }

        return cts.Token;
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
    protected virtual void Dispose(bool disposing)
    {
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
