
using DisposableHelpers.Attributes;

CancellationTokenSource cts = new();
Console.CancelKeyPress += (s, e) =>
{
    e.Cancel = true; // Prevent the process from terminating.
    cts.Cancel();
};


var baseDisposable = new TryDisposable();

while (!cts.IsCancellationRequested)
{
    Spawn1();
    Spawn2();
    await Task.Delay(10);
}

async void Spawn1()
{
    await Task.Yield();
    using var newDisposable = new TryDisposable();
    var ct = newDisposable.CancelWhenDisposed(baseDisposable.CancelWhenDisposed());
    await Task.Delay(10000);
}

async void Spawn2()
{
    await Task.Yield();
    using var newDisposable = new TryDisposable();
    var ct = baseDisposable.CancelWhenDisposed(newDisposable.CancelWhenDisposed());
    await Task.Delay(10000);
}

Console.WriteLine("Cancellation requested. Exiting...");

[Disposable]
public partial class TryDisposable
{
}
