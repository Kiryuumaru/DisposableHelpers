using DisposableHelpers;
using DisposableHelpers.Attributes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace DisposableHelpersTest
{
    [Disposable]
    public partial class BaseDisposeTest
    {
    }

    [Disposable]
    public partial class BaseExistingDisposeTest : MemoryStream
    {
    }

    public class DisposeAnonimousTest
    {
        [Fact]
        public async Task Anonimous()
        {
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
            CancellationToken ctsDisposing1 = dispose.CancelWhenDisposing();
            CancellationToken ctsDisposed1 = dispose.CancelWhenDisposed();
            CancellationToken ctsDisposing2 = dispose.CancelWhenDisposing(new CancellationTokenSource().Token);
            CancellationToken ctsDisposed2 = dispose.CancelWhenDisposed(new CancellationTokenSource().Token);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.False(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
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
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
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
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.True(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.True(ctsDisposed2.IsCancellationRequested);
        }

        [Fact]
        public async Task AnonimousAsync()
        {
            int disposeCallCount = 0;
            int disposingCallCount = 0;
            var dispose = new Disposable(async disposing =>
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
            CancellationToken ctsDisposing1 = dispose.CancelWhenDisposing();
            CancellationToken ctsDisposed1 = dispose.CancelWhenDisposed();
            CancellationToken ctsDisposing2 = dispose.CancelWhenDisposing(new CancellationTokenSource().Token);
            CancellationToken ctsDisposed2 = dispose.CancelWhenDisposed(new CancellationTokenSource().Token);
            Assert.Equal(0, disposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.False(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
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
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
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
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.True(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.True(ctsDisposed2.IsCancellationRequested);
        }
    }

    public class DisposeTest : BaseDisposeTest
    {
        public int DisposeCallCount { get; private set; } = 0;

        public int DisposeAsyncCallCount { get; private set; } = 0;

        protected override void Dispose(bool disposing)
        {
            Thread.Sleep(1000);
            if (disposing)
            {
                DisposeCallCount++;
            }
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            await Task.Delay(1000);
            if (disposing)
            {
                DisposeAsyncCallCount++;
            }
        }

        [Fact]
        public async Task Normal()
        {
            int disposingCallCount = 0;
            var dispose = new DisposeTest();
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            CancellationToken ctsDisposing1 = dispose.CancelWhenDisposing();
            CancellationToken ctsDisposed1 = dispose.CancelWhenDisposed();
            CancellationToken ctsDisposing2 = dispose.CancelWhenDisposing(new CancellationTokenSource().Token);
            CancellationToken ctsDisposed2 = dispose.CancelWhenDisposed(new CancellationTokenSource().Token);
            Assert.Equal(0, dispose.DisposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.False(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposing));
            Assert.Null(Record.Exception(dispose.VerifyNotDisposedOrDisposing));
            Assert.Null(Record.Exception(dispose.VerifyNotDisposed));
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
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposed));
            await Task.Delay(2500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.True(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.True(ctsDisposed2.IsCancellationRequested);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposing));
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            runVoid();
            await Task.Delay(2500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, dispose.DisposeAsyncCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
        }
    }

    public class ExistingDisposeTest : BaseExistingDisposeTest
    {
        public int DisposeCallCount { get; private set; } = 0;

        public int DisposeAsyncCallCount { get; private set; } = 0;

        protected override void Dispose(bool disposing)
        {
            Thread.Sleep(1000);
            if (disposing)
            {
                DisposeCallCount++;
            }
        }

        protected override async ValueTask DisposeAsync(bool disposing)
        {
            await Task.Delay(1000);
            if (disposing)
            {
                DisposeAsyncCallCount++;
            }
        }

        [Fact]
        public async Task Normal()
        {
            int disposingCallCount = 0;
            var dispose = new ExistingDisposeTest();
            MemoryStream disposeParent = dispose;
            dispose.Disposing += (s, e) =>
            {
                disposingCallCount++;
            };
            CancellationToken ctsDisposing1 = dispose.CancelWhenDisposing();
            CancellationToken ctsDisposed1 = dispose.CancelWhenDisposed();
            CancellationToken ctsDisposing2 = dispose.CancelWhenDisposing(new CancellationTokenSource().Token);
            CancellationToken ctsDisposed2 = dispose.CancelWhenDisposed(new CancellationTokenSource().Token);
            Assert.Equal(0, dispose.DisposeCallCount);
            Assert.Equal(0, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.False(dispose.IsDisposedOrDisposing);
            Assert.False(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.False(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposing));
            Assert.Null(Record.Exception(dispose.VerifyNotDisposedOrDisposing));
            Assert.Null(Record.Exception(dispose.VerifyNotDisposed));
            Task run = Task.Run(async delegate
            {
                await Task.Delay(1).ConfigureAwait(false);
                disposeParent.Dispose();
            });
            async void runVoid() => await run;
            runVoid();
            await Task.Delay(100);
            Assert.Equal(0, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.False(dispose.IsDisposed);
            Assert.True(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.False(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.False(ctsDisposed2.IsCancellationRequested);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposed));
            await Task.Delay(2500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.True(dispose.IsDisposed);
            Assert.False(dispose.IsDisposing);
            Assert.True(dispose.IsDisposedOrDisposing);
            Assert.True(ctsDisposing1.IsCancellationRequested);
            Assert.True(ctsDisposed1.IsCancellationRequested);
            Assert.True(ctsDisposing2.IsCancellationRequested);
            Assert.True(ctsDisposed2.IsCancellationRequested);
            Assert.Null(Record.Exception(dispose.VerifyNotDisposing));
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            runVoid();
            await Task.Delay(2500);
            Assert.Equal(1, dispose.DisposeCallCount);
            Assert.Equal(1, dispose.DisposeAsyncCallCount);
            Assert.Equal(1, disposingCallCount);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposedOrDisposing);
            Assert.Throws<ObjectDisposedException>(dispose.VerifyNotDisposed);
        }
    }
}