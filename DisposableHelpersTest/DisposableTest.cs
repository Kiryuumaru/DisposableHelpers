using DisposableHelpers;
using DisposableHelpers.Attributes;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DisposableHelpersTest
{
    [Disposable]
    public partial class BaseDisposeTest
    {
        public BaseDisposeTest()
        {
            
        }
    }

    [AsyncDisposable]
    public partial class BaseAsyncDisposeTest
    {

    }

    public class DisposeTest : BaseDisposeTest
    {
        public int DisposeCallCount { get; private set; } = 0;

        protected override void Dispose(bool disposing)
        {
            Thread.Sleep(1000);
            if (disposing)
            {
                DisposeCallCount++;
            }
            base.Dispose(disposing);
        }

        [Fact]
        public async Task Normal()
        {
            CancellationTokenSource ctsDisposing = new();
            CancellationTokenSource ctsDisposed = new();
            int disposingCallCount = 0;
            var dispose = new DisposeTest();
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            dispose.CancelWhenDisposing(ctsDisposing);
            dispose.CancelWhenDisposed(ctsDisposed);
            Assert.Equal(0, dispose.DisposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Task run = Task.Run(async delegate
            {
                await Task.Delay(1).ConfigureAwait(false);
                dispose.Dispose();
            });
            async void runVoid() => await run;
            runVoid();
            await Task.Delay(100);
            Assert.Equal(0, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.True(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            await Task.Delay(1500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.True(ctsDisposed.IsCancellationRequested);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            runVoid();
            await Task.Delay(1500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
        }

        [Fact]
        public async Task Anonimous()
        {
            CancellationTokenSource ctsDisposing = new();
            CancellationTokenSource ctsDisposed = new();
            int disposeCallCount = 0;
            int disposingCallCount = 0;
            var dispose = new Disposable(disposing =>
            {
                Thread.Sleep(1000);
                if (disposing)
                {
                    disposeCallCount++;
                }
            });
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            dispose.CancelWhenDisposing(ctsDisposing);
            dispose.CancelWhenDisposed(ctsDisposed);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Task run = Task.Run(async delegate
            {
                await Task.Delay(1).ConfigureAwait(false);
                dispose.Dispose();
            });
            async void runVoid() => await run;
            runVoid();
            await Task.Delay(100);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.True(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            await Task.Delay(1500);
            Assert.Equal(1, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            runVoid();
            await Task.Delay(1500);
            Assert.Equal(1, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.True(ctsDisposed.IsCancellationRequested);
        }
    }

    public class AsyncDisposeTest : BaseAsyncDisposeTest
    {
        public int DisposedCallCount { get; private set; } = 0;

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            await Task.Delay(1000);
            if (disposing)
            {
                DisposedCallCount++;
            }
            await base.DisposeAsync(disposing);
        }

        [Fact]
        public async Task Normal()
        {
            CancellationTokenSource ctsDisposing = new();
            CancellationTokenSource ctsDisposed = new();
            int disposingCallCount = 0;
            var dispose = new AsyncDisposeTest();
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            dispose.CancelWhenDisposing(ctsDisposing);
            dispose.CancelWhenDisposed(ctsDisposed);
            Assert.Equal(0, dispose.DisposedCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Task run = Task.Run(async delegate
            {
                await Task.Delay(1).ConfigureAwait(false);
                await dispose.DisposeAsync();
            });
            async void runVoid() => await run;
            runVoid();
            await Task.Delay(100);
            Assert.Equal(0, dispose.DisposedCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.True(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            await Task.Delay(1500);
            Assert.Equal(1, dispose.DisposedCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            runVoid();
            await Task.Delay(1500);
            Assert.Equal(1, dispose.DisposedCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.True(ctsDisposed.IsCancellationRequested);
        }

        [Fact]
        public async Task Anonimous()
        {
            CancellationTokenSource ctsDisposing = new();
            CancellationTokenSource ctsDisposed = new();
            int disposeCallCount = 0;
            int disposingCallCount = 0;
            var dispose = new AsyncDisposable(async disposing =>
            {
                await Task.Delay(1000);
                if (disposing)
                {
                    disposeCallCount++;
                }
            });
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            dispose.CancelWhenDisposing(ctsDisposing);
            dispose.CancelWhenDisposed(ctsDisposed);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            Task run = Task.Run(async delegate
            {
                await Task.Delay(1).ConfigureAwait(false);
                await dispose.DisposeAsync();
            });
            async void runVoid() => await run;
            runVoid();
            await Task.Delay(100);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.True(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.False(ctsDisposed.IsCancellationRequested);
            await Task.Delay(1500);
            Assert.Equal(1, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            runVoid();
            await Task.Delay(1500);
            Assert.Equal(1, disposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(ctsDisposing.IsCancellationRequested);
            Assert.True(ctsDisposed.IsCancellationRequested);
        }
    }
}