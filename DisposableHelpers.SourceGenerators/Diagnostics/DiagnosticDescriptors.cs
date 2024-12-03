// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

#pragma warning disable IDE0090 // Use 'new DiagnosticDescriptor(...)'

namespace DisposableHelpers.SourceGenerators.Diagnostics;

/// <summary>
/// A container for all <see cref="DiagnosticDescriptor"/> instances for errors reported by analyzers in this project.
/// </summary>
internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor UnsupportedCSharpLanguageVersionError = new DiagnosticDescriptor(
        id: "DH0001",
        title: "Unsupported C# language version",
        messageFormat: "The source generator features require consuming projects to set the C# language version to at least C# 8.0",
        category: typeof(CSharpParseOptions).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The source generator features require consuming projects to set the C# language version to at least C# 8.0. Make sure to add <LangVersion>8.0</LangVersion> (or above) to your .csproj file.",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");

    public static readonly DiagnosticDescriptor InvalidAttributeCombinationForDisposableAttributeError = new DiagnosticDescriptor(
        id: "DH0002",
        title: "Invalid target type for [Disposable]",
        messageFormat: "Cannot apply [Disposable] to type {0}, as it already has this attribute or [Disposable] applied to it (including base types)",
        category: typeof(DisposableGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [Disposable] to a type that already has this attribute or [Disposable] applied to it (including base types).",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");

    public static readonly DiagnosticDescriptor TargetHasDirectDisposeImplementationError = new DiagnosticDescriptor(
        id: "DH0003",
        title: "Target type for [Disposable] has direct \"Dispose()\" and \"Dispose(bool)\" implementations",
        messageFormat: "Cannot apply [Disposable] to type {0}, as it already has a \"Dispose()\" and \"Dispose(bool)\" direct method implementations",
        category: typeof(DisposableGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [Disposable] to type, as it already has a \"Dispose()\" and \"Dispose(bool)\" direct method implementations.",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");

    public static readonly DiagnosticDescriptor TargetHasDirectDisposeAsyncImplementationError = new DiagnosticDescriptor(
        id: "DH0004",
        title: "Target type for [Disposable] has direct \"DisposeAsync()\" and \"DisposeAsync(bool)\" implementations",
        messageFormat: "Cannot apply [Disposable] to type {0}, as it already has a \"DisposeAsync()\" and \"DisposeAsync(bool)\" direct method implementations",
        category: typeof(DisposableGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [Disposable] to type, as it already has a \"DisposeAsync()\" and \"DisposeAsync(bool)\" direct method implementations.",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");

    public static readonly DiagnosticDescriptor TargetBaseNoOverridableDisposeMethodError = new DiagnosticDescriptor(
        id: "DH0005",
        title: "Target type for [Disposable] base class has no overridable \"Dispose()\" or \"Dispose(bool)\" implementations",
        messageFormat: "Cannot apply [Disposable] to type {0}, as it has no overridable \"Dispose()\" or \"Dispose(bool)\" implementations",
        category: typeof(DisposableGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [Disposable] to type, as it has no overridable \"Dispose()\" or \"Dispose(bool)\" implementations.",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");

    public static readonly DiagnosticDescriptor TargetBaseNoOverridableDisposeAsyncMethodError = new DiagnosticDescriptor(
        id: "DH0006",
        title: "Target type for [Disposable] base class has no overridable \"DisposeAsync()\" or \"DisposeAsync(bool)\" implementations",
        messageFormat: "Cannot apply [Disposable] to type {0}, as it has no overridable \"DisposeAsync()\" or \"DisposeAsync(bool)\" implementations",
        category: typeof(DisposableGenerator).FullName,
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Cannot apply [Disposable] to type, as it has no overridable \"DisposeAsync()\" or \"DisposeAsync(bool)\" implementations.",
        helpLinkUri: "https://github.com/Kiryuumaru/DisposableHelpers");
}
