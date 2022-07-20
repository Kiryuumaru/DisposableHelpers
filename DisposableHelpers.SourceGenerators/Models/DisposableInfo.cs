// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;

namespace DisposableHelpers.SourceGenerators.ComponentModel.Models;

public sealed record DisposableInfo(
    string TypeName,
    bool HasExplicitDestructors,
    bool HasImplementedIDisposable,
    IMethodSymbol? DisposeMethod);
