# DisposableHelpers

Disposable helpers for IDisposable and IAsyncDisposable with source generators. Also capable of both anonymous disposable and anonymous async disposable.

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|DisposableHelpers|[![NuGet](https://buildstats.info/nuget/DisposableHelpers?includePreReleases=true)](https://www.nuget.org/packages/DisposableHelpers/)|

## Installation
```csharp
// Install release version
Install-Package DisposableHelpers

// Install pre-release version
Install-Package DisposableHelpers -pre
```

## Supported frameworks
.NET Standard 2.0 and above - see https://github.com/dotnet/standard/blob/master/docs/versions.md for compatibility matrix

## Usage

### Disposable Sample 1
```csharp
using DisposableHelpers;

namespace YourNamespace
{
    public class SampleDisposable : Disposable
    {
        private SampleUnmanagedResource resources;
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                resources.Release();
            }
            base.Dispose(disposing);
        }
    }
}
```
### Disposable Sample 2
```csharp
using DisposableHelpers.Attributes;

namespace YourNamespace
{
    [Disposable]
    public class SampleDisposable
    {
        private SampleUnmanagedResource resources;
        
        protected void Dispose(bool disposing)
        {
            if (disposing)
            {
                resources.Release();
            }
            base.Dispose(disposing);
        }
    }
}
```
### Anonymous Disposable Sample
```csharp
using DisposableHelpers;

namespace YourNamespace
{
    public static class Program
    {
        private static SampleUnmanagedResource resources;
        
        public static void Main(string[] args)
        {
            Disposable disposable = new Disposable(disposing =>
            {
                if (disposing)
                {
                    resources.Release();
                }
            });

            disposable.Dispose();
        }
    }
}
```
### AsyncDisposable Sample 1
```csharp
using DisposableHelpers;

namespace YourNamespace
{
    public class SampleAsyncDisposable : AsyncDisposable
    {
        private SampleAsyncUnmanagedResource resources;
        
        protected override async ValueTask Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                await resources.Release();
            }
            return base.Dispose(isDisposing);
        }
    }
}
```
### AsyncDisposable Sample 2
```csharp
using DisposableHelpers.Attributes;

namespace YourNamespace
{
    [AsyncDisposable]
    public class SampleAsyncDisposable
    {
        private SampleAsyncUnmanagedResource resources;
        
        protected async ValueTask Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                await resources.Release();
            }
            return base.Dispose(isDisposing);
        }
    }
}
```
### Anonymous AsyncDisposable Sample
```csharp
using DisposableHelpers;

namespace YourNamespace
{
    public static class Program
    {
        private static SampleAsyncUnmanagedResource resources;
        
        public static async void Main(string[] args)
        {
            AsyncDisposable disposable = new AsyncDisposable(async disposing =>
            {
                if (disposing)
                {
                    await resources.Release();
                }
            });

            await disposable.DisposeAsync();
        }
    }
}
```
### Want To Support This Project?
All I have ever asked is to be active by submitting bugs, features, and sending those pull requests down!.
