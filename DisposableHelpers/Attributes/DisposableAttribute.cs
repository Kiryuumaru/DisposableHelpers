using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace DisposableHelpers.Attributes;

/// <summary>
/// Contains all methods for performing proper <see cref="IDisposable"/> operations.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
[Conditional("MVVMTOOLKIT_KEEP_SOURCE_GENERATOR_ATTRIBUTES")]
public sealed class DisposableAttribute : Attribute
{
}
