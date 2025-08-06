using DisposableHelpers;
using DisposableHelpers.Attributes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DisposableHelpersTest
{
    // Test helper classes for various inheritance scenarios
    public class BaseTryDisposable1 : IDisposable, IAsyncDisposable
    {
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public virtual ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            throw new NotImplementedException();
        }
    }

    [Disposable]
    public partial class TryDisposable11 : BaseTryDisposable1
    {
    }

    [Disposable]
    public partial class TryDisposable12 : BaseTryDisposable1
    {
        public override void Dispose()
        {
            base.Dispose();
        }
        protected override ValueTask DisposeAsync(bool disposing)
        {
            return base.DisposeAsync(disposing);
        }
    }

    [Disposable]
    public partial class TryDisposable13 : BaseTryDisposable1
    {
        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    public class BaseTryDisposable2 : IDisposable, IAsyncDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            throw new NotImplementedException();
        }
    }

    [Disposable]
    public partial class TryDisposable21 : BaseTryDisposable2
    {
    }

    [Disposable]
    public partial class TryDisposable22 : BaseTryDisposable2
    {
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }

    [Disposable]
    public partial class TryDisposable23 : BaseTryDisposable2
    {
        protected override ValueTask DisposeAsync(bool disposing)
        {
            return base.DisposeAsync(disposing);
        }
    }

    public class BaseTryDisposable3 : IDisposable, IAsyncDisposable
    {
        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        protected void Dispose(bool disposing)
        {
            throw new NotImplementedException();
        }

        public virtual ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        protected ValueTask DisposeAsync(bool disposing)
        {
            throw new NotImplementedException();
        }
    }

    [Disposable]
    public partial class TryDisposable31 : BaseTryDisposable3
    {
    }

    [Disposable]
    public partial class TryDisposable32 : BaseTryDisposable3
    {
        public override void Dispose()
        {
            base.Dispose();
        }
    }

    [Disposable]
    public partial class TryDisposable33 : BaseTryDisposable3
    {
        public override ValueTask DisposeAsync()
        {
            return base.DisposeAsync();
        }
    }

    [Disposable]
    public partial class BaseDisposeTest
    {
    }

    [Disposable]
    public partial class BaseExistingDisposeTest : MemoryStream
    {
    }

    // Test classes for specific functionality testing
    public class TestDisposable : BaseDisposeTest
    {
        public bool DisposeWasCalled { get; private set; }
        public bool DisposeAsyncWasCalled { get; private set; }
        public int DisposeCallCount { get; private set; }
        public int DisposeAsyncCallCount { get; private set; }
        public ManualResetEventSlim DisposeStarted { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DisposeCompleted { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DisposeAsyncStarted { get; } = new ManualResetEventSlim(false);
        public ManualResetEventSlim DisposeAsyncCompleted { get; } = new ManualResetEventSlim(false);

        // Public wrappers for protected methods to enable testing
        public void PublicVerifyNotDisposing() => VerifyNotDisposing();
        public void PublicVerifyNotDisposed() => VerifyNotDisposed();
        public void PublicVerifyNotDisposedOrDisposing() => VerifyNotDisposedOrDisposing();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeStarted.Set();
                DisposeWasCalled = true;
                DisposeCallCount++;
                DisposeCompleted.Set();
            }
            base.Dispose(disposing);
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                DisposeAsyncStarted.Set();
                DisposeAsyncWasCalled = true;
                DisposeAsyncCallCount++;
                // Small delay to ensure async behavior
                await Task.Yield();
                DisposeAsyncCompleted.Set();
            }
            await base.DisposeAsync(disposing);
        }
    }

    public class AnonymousDisposableTests
    {
        [Fact]
        public void AnonymousDisposable_WithSyncAction_DisposesCorrectly()
        {
            bool disposeWasCalled = false;
            var disposable = new Disposable(disposing =>
            {
                if (disposing)
                {
                    disposeWasCalled = true;
                }
            });

            // Initial state
            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.False(disposable.IsDisposedOrDisposing);
            Assert.False(disposeWasCalled);

            // Dispose
            disposable.Dispose();

            // Final state
            Assert.True(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposedOrDisposing);
            Assert.True(disposeWasCalled);
        }

        [Fact]
        public void AnonymousDisposable_WithAsyncAction_DisposesCorrectly()
        {
            bool disposeWasCalled = false;
            var disposable = new Disposable(async disposing =>
            {
                if (disposing)
                {
                    await Task.Yield();
                    disposeWasCalled = true;
                }
            });

            // Initial state
            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.False(disposable.IsDisposedOrDisposing);
            Assert.False(disposeWasCalled);

            // Dispose
            disposable.Dispose();

            // Final state
            Assert.True(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposedOrDisposing);
            Assert.True(disposeWasCalled);
        }

        [Fact]
        public async Task AnonymousDisposable_WithAsyncAction_DisposeAsyncCorrectly()
        {
            bool disposeWasCalled = false;
            var disposable = new Disposable(async disposing =>
            {
                if (disposing)
                {
                    await Task.Yield();
                    disposeWasCalled = true;
                }
            });

            // Initial state
            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.False(disposable.IsDisposedOrDisposing);
            Assert.False(disposeWasCalled);

            // Dispose async
            await disposable.DisposeAsync();

            // Final state
            Assert.True(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposedOrDisposing);
            Assert.True(disposeWasCalled);
        }

        [Fact]
        public void AnonymousDisposable_DisposeMultipleTimes_OnlyDisposesOnce()
        {
            int disposeCallCount = 0;
            var disposable = new Disposable(disposing =>
            {
                if (disposing)
                {
                    disposeCallCount++;
                }
            });

            disposable.Dispose();
            disposable.Dispose();
            disposable.Dispose();

            Assert.Equal(1, disposeCallCount);
            Assert.True(disposable.IsDisposed);
        }
    }

    public class DisposableStateTests
    {
        [Fact]
        public void InitialState_ShouldBeNotDisposed()
        {
            var disposable = new TestDisposable();

            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.False(disposable.IsDisposedOrDisposing);
            Assert.False(disposable.DisposeWasCalled);
            Assert.False(disposable.DisposeAsyncWasCalled);
            Assert.Equal(0, disposable.DisposeCallCount);
            Assert.Equal(0, disposable.DisposeAsyncCallCount);
        }

        [Fact]
        public void AfterDispose_ShouldBeDisposed()
        {
            var disposable = new TestDisposable();

            disposable.Dispose();

            Assert.True(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposedOrDisposing);
            Assert.True(disposable.DisposeWasCalled);
            Assert.Equal(1, disposable.DisposeCallCount);
        }

        [Fact]
        public async Task AfterDisposeAsync_ShouldBeDisposed()
        {
            var disposable = new TestDisposable();

            await disposable.DisposeAsync();

            Assert.True(disposable.IsDisposed);
            Assert.False(disposable.IsDisposing);
            Assert.True(disposable.IsDisposedOrDisposing);
            Assert.True(disposable.DisposeAsyncWasCalled);
            Assert.Equal(1, disposable.DisposeAsyncCallCount);
        }

        [Fact]
        public void MultipleDispose_OnlyDisposesOnce()
        {
            var disposable = new TestDisposable();

            disposable.Dispose();
            disposable.Dispose();
            disposable.Dispose();

            Assert.Equal(1, disposable.DisposeCallCount);
            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public async Task MultipleDisposeAsync_OnlyDisposesOnce()
        {
            var disposable = new TestDisposable();

            await disposable.DisposeAsync();
            await disposable.DisposeAsync();
            await disposable.DisposeAsync();

            Assert.Equal(1, disposable.DisposeAsyncCallCount);
            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public async Task MixedDisposeAndDisposeAsync_OnlyDisposesOnce()
        {
            var disposable = new TestDisposable();

            disposable.Dispose();
            await disposable.DisposeAsync();

            // Both sync and async dispose should have been called once each
            Assert.Equal(1, disposable.DisposeCallCount);
            Assert.Equal(1, disposable.DisposeAsyncCallCount);
            Assert.True(disposable.IsDisposed);
        }
    }

    public class DisposingEventTests
    {
        [Fact]
        public void DisposingEvent_FiresBeforeDispose()
        {
            var disposable = new TestDisposable();
            bool disposingEventFired = false;
            bool disposeWasCalledWhenEventFired = false;

            disposable.Disposing += (sender, e) =>
            {
                disposingEventFired = true;
                disposeWasCalledWhenEventFired = disposable.DisposeWasCalled;
            };

            disposable.Dispose();

            Assert.True(disposingEventFired);
            Assert.False(disposeWasCalledWhenEventFired); // Event should fire before Dispose method
            Assert.True(disposable.DisposeWasCalled); // But Dispose should be called after event
        }

        [Fact]
        public async Task DisposingEvent_FiresBeforeDisposeAsync()
        {
            var disposable = new TestDisposable();
            bool disposingEventFired = false;
            bool disposeAsyncWasCalledWhenEventFired = false;

            disposable.Disposing += (sender, e) =>
            {
                disposingEventFired = true;
                disposeAsyncWasCalledWhenEventFired = disposable.DisposeAsyncWasCalled;
            };

            await disposable.DisposeAsync();

            Assert.True(disposingEventFired);
            Assert.False(disposeAsyncWasCalledWhenEventFired); // Event should fire before DisposeAsync method
            Assert.True(disposable.DisposeAsyncWasCalled); // But DisposeAsync should be called after event
        }

        [Fact]
        public void DisposingEvent_OnlyFiresOnce()
        {
            var disposable = new TestDisposable();
            int eventFireCount = 0;

            disposable.Disposing += (sender, e) => eventFireCount++;

            disposable.Dispose();
            disposable.Dispose();

            Assert.Equal(1, eventFireCount);
        }
    }

    public class CancellationTokenTests
    {
        [Fact]
        public void CancelWhenDisposing_CancelsOnDispose()
        {
            var disposable = new TestDisposable();
            var token = disposable.CancelWhenDisposing();

            Assert.False(token.IsCancellationRequested);

            disposable.Dispose();

            Assert.True(token.IsCancellationRequested);
        }

        [Fact]
        public void CancelWhenDisposed_CancelsAfterDispose()
        {
            var disposable = new TestDisposable();
            var token = disposable.CancelWhenDisposed();

            Assert.False(token.IsCancellationRequested);

            disposable.Dispose();

            Assert.True(token.IsCancellationRequested);
        }

        [Fact]
        public async Task CancelWhenDisposing_CancelsOnDisposeAsync()
        {
            var disposable = new TestDisposable();
            var token = disposable.CancelWhenDisposing();

            Assert.False(token.IsCancellationRequested);

            await disposable.DisposeAsync();

            Assert.True(token.IsCancellationRequested);
        }

        [Fact]
        public async Task CancelWhenDisposed_CancelsAfterDisposeAsync()
        {
            var disposable = new TestDisposable();
            var token = disposable.CancelWhenDisposed();

            Assert.False(token.IsCancellationRequested);

            await disposable.DisposeAsync();

            Assert.True(token.IsCancellationRequested);
        }

        [Fact]
        public void CancelWhenDisposing_WithExistingToken_CombinesCorrectly()
        {
            var disposable = new TestDisposable();
            using var cts = new CancellationTokenSource();
            var combinedToken = disposable.CancelWhenDisposing(cts.Token);

            Assert.False(combinedToken.IsCancellationRequested);

            cts.Cancel();
            Assert.True(combinedToken.IsCancellationRequested);
        }

        [Fact]
        public void CancelWhenDisposed_WithExistingToken_CombinesCorrectly()
        {
            var disposable = new TestDisposable();
            using var cts = new CancellationTokenSource();
            var combinedToken = disposable.CancelWhenDisposed(cts.Token);

            Assert.False(combinedToken.IsCancellationRequested);

            cts.Cancel();
            Assert.True(combinedToken.IsCancellationRequested);
        }
    }

    public class VerificationMethodTests
    {
        [Fact]
        public void VerifyNotDisposing_WhenNotDisposing_DoesNotThrow()
        {
            var disposable = new TestDisposable();
            var exception = Record.Exception(() => disposable.PublicVerifyNotDisposing());
            Assert.Null(exception);
        }

        [Fact]
        public void VerifyNotDisposed_WhenNotDisposed_DoesNotThrow()
        {
            var disposable = new TestDisposable();
            var exception = Record.Exception(() => disposable.PublicVerifyNotDisposed());
            Assert.Null(exception);
        }

        [Fact]
        public void VerifyNotDisposedOrDisposing_WhenNotDisposedOrDisposing_DoesNotThrow()
        {
            var disposable = new TestDisposable();
            var exception = Record.Exception(() => disposable.PublicVerifyNotDisposedOrDisposing());
            Assert.Null(exception);
        }

        [Fact]
        public void VerifyNotDisposed_WhenDisposed_Throws()
        {
            var disposable = new TestDisposable();
            disposable.Dispose();

            Assert.Throws<ObjectDisposedException>(() => disposable.PublicVerifyNotDisposed());
        }

        [Fact]
        public void VerifyNotDisposedOrDisposing_WhenDisposed_Throws()
        {
            var disposable = new TestDisposable();
            disposable.Dispose();

            Assert.Throws<ObjectDisposedException>(() => disposable.PublicVerifyNotDisposedOrDisposing());
        }

        [Fact]
        public void VerifyNotDisposing_WhenDisposed_DoesNotThrow()
        {
            var disposable = new TestDisposable();
            disposable.Dispose();

            // After disposal is complete, IsDisposing should be false
            var exception = Record.Exception(() => disposable.PublicVerifyNotDisposing());
            Assert.Null(exception);
        }
    }

    // Concurrency tests with proper synchronization
    public class ConcurrencyTests
    {
        [Fact]
        public async Task ConcurrentDispose_OnlyDisposesOnce()
        {
            const int threadCount = 10;
            var disposable = new TestDisposable();
            var tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() => disposable.Dispose());
            }

            await Task.WhenAll(tasks);

            Assert.Equal(1, disposable.DisposeCallCount);
            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public async Task ConcurrentDisposeAsync_OnlyDisposesOnce()
        {
            const int threadCount = 10;
            var disposable = new TestDisposable();
            var tasks = new Task[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(async () => await disposable.DisposeAsync());
            }

            await Task.WhenAll(tasks);

            Assert.Equal(1, disposable.DisposeAsyncCallCount);
            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public async Task MixedConcurrentDispose_OnlyDisposesOnce()
        {
            const int threadCount = 5;
            var disposable = new TestDisposable();
            var tasks = new Task[threadCount * 2];

            // Half sync dispose, half async dispose
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = Task.Run(() => disposable.Dispose());
                tasks[i + threadCount] = Task.Run(async () => await disposable.DisposeAsync());
            }

            await Task.WhenAll(tasks);

            // Should dispose once, but both sync and async dispose methods should be called
            Assert.Equal(1, disposable.DisposeCallCount);
            Assert.Equal(1, disposable.DisposeAsyncCallCount);
            Assert.True(disposable.IsDisposed);
        }
    }

    public class ExistingDisposeTest : BaseExistingDisposeTest
    {
        public bool DisposeWasCalled { get; private set; }
        public bool DisposeAsyncWasCalled { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeWasCalled = true;
            }
            base.Dispose(disposing);
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                DisposeAsyncWasCalled = true;
            }
            await base.DisposeAsync(disposing);
        }

        [Fact]
        public void InheritedFromMemoryStream_DisposesCorrectly()
        {
            var disposable = new ExistingDisposeTest();

            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.DisposeWasCalled);

            disposable.Dispose();

            Assert.True(disposable.IsDisposed);
            Assert.True(disposable.DisposeWasCalled);
        }

        [Fact]
        public async Task InheritedFromMemoryStream_DisposeAsyncCorrectly()
        {
            var disposable = new ExistingDisposeTest();

            Assert.False(disposable.IsDisposed);
            Assert.False(disposable.DisposeAsyncWasCalled);

            await disposable.DisposeAsync();

            Assert.True(disposable.IsDisposed);
            Assert.True(disposable.DisposeAsyncWasCalled);
        }
    }
}