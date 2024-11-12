using DisposableHelpers.Attributes;
using System;
using System.Threading.Tasks;

namespace DisposableHelpers;

/// <summary>
/// Contains all methods for performing proper <see cref="IDisposable"/> operations.
/// </summary>
[Disposable]
public partial class Disposable
{
    private readonly Action<bool>? _dispose;
    private readonly Func<bool, ValueTask>? _disposeAsync;

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
        _dispose = dispose;
    }

    /// <summary>
    /// Creates an instance of <see cref="Disposable"/> class with async function on dispose.
    /// </summary>
    public Disposable(Func<bool, ValueTask> disposeAsync)
    {
        _disposeAsync = disposeAsync;
    }

    /// <summary>
    /// Allows subclasses to provide dispose logic.
    /// </summary>
    /// <param name="disposing">
    /// Whether the method is being called in response to disposal, or finalization.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        _dispose?.Invoke(disposing);
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
    protected virtual async ValueTask DisposeAsync(bool disposing)
    {
        if (_disposeAsync != null)
        {
            await _disposeAsync.Invoke(disposing);
        }
    }
}
