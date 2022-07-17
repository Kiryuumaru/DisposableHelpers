using DisposableHelpers.Attributes;
using System;
using System.Threading.Tasks;

namespace DisposableHelpers;

/// <summary>
/// Contains all methods for performing proper <see cref="IAsyncDisposable"/> operations.
/// </summary>
[AsyncDisposable]
public partial class AsyncDisposable
{
    private readonly Func<bool, ValueTask>? dispose;

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
        if (dispose != null)
        {
            await dispose.Invoke(disposing);
        }
    }
}
