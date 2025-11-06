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
    private readonly global::System.Collections.Generic.Dictionary<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration> linkedCancellationTokenSources = new global::System.Collections.Generic.Dictionary<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration>();
    private readonly object linkedCancellationTokenSourcesLock = new object();

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

        cancelOnDisposingCts.Cancel();
        OnDisposing();
        Disposing = null;

        // Dispose linked CTS before calling user dispose logic to prevent additions during disposal
        global::System.Collections.Generic.List<global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration>> ctsToDispose;
        lock (linkedCancellationTokenSourcesLock)
        {
            ctsToDispose = new global::System.Collections.Generic.List<global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration>>(linkedCancellationTokenSources);
            linkedCancellationTokenSources.Clear();
        }

        foreach (global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration> kvp in ctsToDispose)
        {
            try
            {
                kvp.Value.Dispose();
                kvp.Key?.Dispose();
            }
            catch
            {
                // Suppress exceptions during disposal
            }
        }

        Dispose(true);
        DisposeAsync(true).AsTask().Wait();
        cancelOnDisposedCts.Cancel();

        cancelOnDisposingCts.Dispose();
        cancelOnDisposedCts.Dispose();

        global::System.GC.SuppressFinalize(this);
        global::System.Threading.Interlocked.Exchange(ref disposeStage, DisposalComplete);
    }

    private async global::System.Threading.Tasks.ValueTask CoreDisposeAsync()
    {
        if (global::System.Threading.Interlocked.CompareExchange(ref disposeStage, DisposalStarted, DisposalNotStarted) != DisposalNotStarted)
        {
            return;
        }

        cancelOnDisposingCts.Cancel();
        OnDisposing();
        Disposing = null;

        // Dispose linked CTS before calling user dispose logic to prevent additions during disposal
        global::System.Collections.Generic.List<global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration>> ctsToDispose;
        lock (linkedCancellationTokenSourcesLock)
        {
            ctsToDispose = new global::System.Collections.Generic.List<global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration>>(linkedCancellationTokenSources);
            linkedCancellationTokenSources.Clear();
        }

        foreach (global::System.Collections.Generic.KeyValuePair<global::System.Threading.CancellationTokenSource, global::System.Threading.CancellationTokenRegistration> kvp in ctsToDispose)
        {
            try
            {
                kvp.Value.Dispose();
                kvp.Key?.Dispose();
            }
            catch
            {
                // Suppress exceptions during disposal
            }
        }

        Dispose(true);
        await DisposeAsync(true);
        cancelOnDisposedCts.Cancel();

        cancelOnDisposingCts.Dispose();
        cancelOnDisposedCts.Dispose();

        global::System.GC.SuppressFinalize(this);
        global::System.Threading.Interlocked.Exchange(ref disposeStage, DisposalComplete);
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    public void Dispose_Normal()
    {
        CoreDispose();
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    public override void Dispose_Override()
    {
        CoreDispose();
        base.Dispose();
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    /// <returns>
    /// The <see cref="global::System.Threading.Tasks.ValueTask"/> of the dispose operation.
    /// </returns>
    public global::System.Threading.Tasks.ValueTask DisposeAsync_Normal()
    {
        return CoreDisposeAsync();
    }

    /// <summary>
    /// Disposes of this object, if it hasn't already been disposed.
    /// </summary>
    /// <returns>
    /// The <see cref="global::System.Threading.Tasks.ValueTask"/> of the dispose operation.
    /// </returns>
    public override async global::System.Threading.Tasks.ValueTask DisposeAsync_Override()
    {
        await CoreDisposeAsync();
        await base.DisposeAsync();
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

        global::System.Threading.CancellationTokenSource cts = global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct.ToArray());

        // Check disposal status while holding lock to prevent race condition
        lock (linkedCancellationTokenSourcesLock)
        {
            if (IsDisposed)
            {
                // Already disposed, cancel and dispose immediately
                cts.Cancel();
                global::System.Threading.CancellationToken token = cts.Token;
                cts.Dispose();
                return token;
            }

            // Register callback to remove and dispose CTS when canceled
            // The callback uses useSynchronizationContext: false to ensure it runs on thread pool
            global::System.Threading.CancellationTokenRegistration registration = cts.Token.Register(
                state =>
                {
                    global::System.Threading.CancellationTokenSource ctsToClean = (global::System.Threading.CancellationTokenSource)state;
                    global::System.Threading.CancellationTokenRegistration regToClean = default;
                    bool found = false;

                    lock (linkedCancellationTokenSourcesLock)
                    {
                        if (linkedCancellationTokenSources.TryGetValue(ctsToClean, out regToClean))
                        {
                            linkedCancellationTokenSources.Remove(ctsToClean);
                            found = true;
                        }
                    }

                    // Dispose the registration first to break the parent->child reference
                    // Then dispose the CTS itself
                    if (found)
                    {
                        try
                        {
                            regToClean.Dispose();
                        }
                        catch
                        {
                            // Suppress exceptions during disposal
                        }

                        try
                        {
                            ctsToClean.Dispose();
                        }
                        catch
                        {
                            // Suppress exceptions during disposal
                        }
                    }
                },
                cts,
                useSynchronizationContext: false);

            linkedCancellationTokenSources.Add(cts, registration);
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

        global::System.Threading.CancellationTokenSource cts = global::System.Threading.CancellationTokenSource.CreateLinkedTokenSource(ct.ToArray());

        // Check disposal status while holding lock to prevent race condition
        lock (linkedCancellationTokenSourcesLock)
        {
            if (IsDisposedOrDisposing)
            {
                // Already disposing/disposed, cancel and dispose immediately
                cts.Cancel();
                global::System.Threading.CancellationToken token = cts.Token;
                cts.Dispose();
                return token;
            }

            // Register callback to remove and dispose CTS when canceled
            // The callback uses useSynchronizationContext: false to ensure it runs on thread pool
            global::System.Threading.CancellationTokenRegistration registration = cts.Token.Register(
                state =>
                {
                    global::System.Threading.CancellationTokenSource ctsToClean = (global::System.Threading.CancellationTokenSource)state;
                    global::System.Threading.CancellationTokenRegistration regToClean = default;
                    bool found = false;

                    lock (linkedCancellationTokenSourcesLock)
                    {
                        if (linkedCancellationTokenSources.TryGetValue(ctsToClean, out regToClean))
                        {
                            linkedCancellationTokenSources.Remove(ctsToClean);
                            found = true;
                        }
                    }

                    // Dispose the registration first to break the parent->child reference
                    // Then dispose the CTS itself
                    if (found)
                    {
                        try
                        {
                            regToClean.Dispose();
                        }
                        catch
                        {
                            // Suppress exceptions during disposal
                        }

                        try
                        {
                            ctsToClean.Dispose();
                        }
                        catch
                        {
                            // Suppress exceptions during disposal
                        }
                    }
                },
                cts,
                useSynchronizationContext: false);

            linkedCancellationTokenSources.Add(cts, registration);
        }

        return cts.Token;
    }

    /// <summary>
    /// Returns a <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is fully disposed.
    /// </summary>
    /// <returns>A <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is disposed.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposed()
    {
        return cancelOnDisposedCts.Token;
    }

    /// <summary>
    /// Returns a <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object starts disposing.
    /// </summary>
    /// <returns>A <see cref="global::System.Threading.CancellationToken"/> that will be canceled when the object is disposing.</returns>
    public global::System.Threading.CancellationToken CancelWhenDisposing()
    {
        return cancelOnDisposingCts.Token;
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
    protected virtual void Dispose_Normal(bool disposing)
    {
    }

    /// <summary>
    /// Allows subclasses to provide dispose logic.
    /// </summary>
    /// <param name="disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected override void Dispose_Override(bool disposing)
    {
        base.Dispose(disposing && IsDisposing);
    }

    /// <summary>
    /// Allows subclasses to provide dispose logic.
    /// </summary>
    /// <param name="disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected override void Dispose_OverrideCross(bool disposing)
    {
        CoreDispose();
        base.Dispose(disposing && IsDisposing);
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
    protected virtual System.Threading.Tasks.ValueTask DisposeAsync_Normal(bool disposing)
    {
        return default;
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
    protected override System.Threading.Tasks.ValueTask DisposeAsync_Override(bool disposing)
    {
        return base.DisposeAsync(disposing && IsDisposing);
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
    protected override async System.Threading.Tasks.ValueTask DisposeAsync_OverrideCross(bool disposing)
    {
        await CoreDisposeAsync();
        await base.DisposeAsync(disposing && IsDisposing);
    }
}
