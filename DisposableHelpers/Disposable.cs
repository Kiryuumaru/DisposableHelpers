using DisposableHelpers.Attributes;
using System;

namespace DisposableHelpers;

/// <summary>
/// Contains all methods for performing proper <see cref="IDisposable"/> operations.
/// </summary>
[Disposable]
public partial class Disposable
{
    private readonly Action<bool>? dispose;

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
    /// Allows subclasses to provide dispose logic.
    /// </summary>
    /// <param name="disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        dispose?.Invoke(disposing);
    }
}
