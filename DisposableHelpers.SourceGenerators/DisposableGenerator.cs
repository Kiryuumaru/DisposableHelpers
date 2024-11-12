using DisposableHelpers.SourceGenerators.ComponentModel.Models;
using DisposableHelpers.SourceGenerators.Diagnostics;
using DisposableHelpers.SourceGenerators.Extensions;
using DisposableHelpers.SourceGenerators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using static DisposableHelpers.SourceGenerators.Diagnostics.DiagnosticDescriptors;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DisposableHelpers.SourceGenerators
{
    [Generator(LanguageNames.CSharp)]
    internal sealed partial class DisposableGenerator : TransitiveMembersGenerator<DisposableInfo>
    {
        public DisposableGenerator()
            : base("global::DisposableHelpers.Attributes.DisposableAttribute")
        {

        }

        protected override IncrementalValuesProvider<(INamedTypeSymbol Symbol, DisposableInfo Info)> GetInfo(
            IncrementalGeneratorInitializationContext context,
            IncrementalValuesProvider<(INamedTypeSymbol Symbol, AttributeData AttributeData)> source)
        {
            static DisposableInfo GetInfo(INamedTypeSymbol typeSymbol, AttributeData attributeData)
            {
                string typeName = typeSymbol.Name;
                bool hasExplicitDestructors = typeSymbol.GetMembers().Any(m => m is IMethodSymbol symbol && symbol.MethodKind == MethodKind.Destructor);

                bool hasImplementedIDisposable = typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::System.IDisposable"));
                var disposeMethod = typeSymbol.GetMembers().FirstOrDefault(i =>
                    i is IMethodSymbol symbol &&
                    symbol.Name == "Dispose" &&
                    symbol.Parameters.Length == 0);
                var disposeBoolMethod = typeSymbol.GetMembers().FirstOrDefault(i =>
                    i is IMethodSymbol symbol &&
                    symbol.Name == "Dispose" &&
                    symbol.Parameters.Length == 1 &&
                    symbol.Parameters[0].Type.Name == typeof(bool).Name);

                bool hasImplementedIAsyncDisposable = typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::System.IAsyncDisposable"));
                var disposeAsyncMethod = typeSymbol.GetMembers().FirstOrDefault(i =>
                    i is IMethodSymbol symbol &&
                    symbol.Name == "DisposeAsync" &&
                    symbol.Parameters.Length == 0);
                var disposeAsyncBoolMethod = typeSymbol.GetMembers().FirstOrDefault(i =>
                    i is IMethodSymbol symbol &&
                    symbol.Name == "DisposeAsync" &&
                    symbol.Parameters.Length == 1 &&
                    symbol.Parameters[0].Type.Name == typeof(bool).Name);

                return new(
                    typeName,
                    hasExplicitDestructors,
                    hasImplementedIDisposable,
                    hasImplementedIAsyncDisposable,
                    disposeMethod as IMethodSymbol,
                    disposeAsyncMethod as IMethodSymbol,
                    disposeBoolMethod as IMethodSymbol,
                    disposeAsyncBoolMethod as IMethodSymbol);
            }

            return
                source
                .Select(static (item, _) => (item.Symbol, GetInfo(item.Symbol, item.AttributeData)));
        }

        protected override bool ValidateTargetType(INamedTypeSymbol typeSymbol, DisposableInfo info, out ImmutableArray<Diagnostic> diagnostics)
        {
            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();

            // Check if the type uses [DisposableAttribute] already (in the type hierarchy too)
            if (typeSymbol.InheritsAttributeWithFullyQualifiedName("global::DisposableHelpers.Attributes.DisposableAttribute"))
            {
                builder.Add(InvalidAttributeCombinationForDisposableAttributeError, typeSymbol, typeSymbol);

                diagnostics = builder.ToImmutable();

                return false;
            }

            diagnostics = builder.ToImmutable();

            return true;
        }

        protected override ImmutableArray<MemberDeclarationSyntax> FilterDeclaredMembers(DisposableInfo info, ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
        {
            ImmutableArray<MemberDeclarationSyntax>.Builder builder = ImmutableArray.CreateBuilder<MemberDeclarationSyntax>();

            // If the target type has no destructors, generate destructors.
            if (!info.HasExplicitDestructors)
            {
                foreach (DestructorDeclarationSyntax ctor in memberDeclarations.OfType<DestructorDeclarationSyntax>())
                {
                    string text = ctor.NormalizeWhitespace().ToFullString();
                    string replaced = text.Replace("~Disposable", $"~{info.TypeName}");

                    Console.WriteLine(text);

                    builder.Add((DestructorDeclarationSyntax)ParseMemberDeclaration(replaced)!);
                }
            }

            MemberDeclarationSyntax? FixupFilteredMemberDeclaration(MemberDeclarationSyntax member)
            {
                // Remove Dispose(bool) if the target type already has the method
                if (info.DisposeBoolMethod != null &&
                    member is MethodDeclarationSyntax disposeBoolSyntax &&
                    disposeBoolSyntax.Identifier.ValueText == "Dispose" &&
                    disposeBoolSyntax.ParameterList.Parameters.Count == 1 &&
                    disposeBoolSyntax.ParameterList.Parameters[0].Type?.ToString() == "bool")
                {
                    return null;
                }

                // Remove DisposeAsync(bool) if the target type already has the method
                if (info.DisposeAsyncBoolMethod != null &&
                    member is MethodDeclarationSyntax disposeAsyncBoolSyntax &&
                    disposeAsyncBoolSyntax.Identifier.ValueText == "DisposeAsync" &&
                    disposeAsyncBoolSyntax.ParameterList.Parameters.Count == 1 &&
                    disposeAsyncBoolSyntax.ParameterList.Parameters[0].Type?.ToString() == "bool")
                {
                    return null;
                }

                // Remove Dispose() if the target type already has the method
                if (info.DisposeMethod != null &&
                    member is MethodDeclarationSyntax disposeSyntax &&
                    disposeSyntax.Identifier.ValueText == "Dispose" &&
                    disposeSyntax.ParameterList.Parameters.Count == 0)
                {
                    return null;
                }

                // Remove DisposeAsync() if the target type already has the method
                if (info.DisposeAsyncMethod != null &&
                    member is MethodDeclarationSyntax disposeAsyncSyntax &&
                    disposeAsyncSyntax.Identifier.ValueText == "DisposeAsync" &&
                    disposeAsyncSyntax.ParameterList.Parameters.Count == 0)
                {
                    return null;
                }

                return member;
            }

            // Generate
            foreach (MemberDeclarationSyntax member in memberDeclarations.Where(static member => member is not DestructorDeclarationSyntax))
            {
                MemberDeclarationSyntax? syntax = FixupFilteredMemberDeclaration(member);
                if (syntax != null)
                {
                    builder.Add(syntax);
                }
            }

            return builder.ToImmutable();
        }

        protected override CompilationUnitSyntax GetCompilationUnit(SourceProductionContext sourceProductionContext, DisposableInfo info, HierarchyInfo hierarchyInfo, bool isSealed, ImmutableArray<MemberDeclarationSyntax> memberDeclarations)
        {
            if (info.HasImplementedIDisposable)
            {
                return hierarchyInfo.GetCompilationUnit(memberDeclarations);
            }
            else
            {
                return hierarchyInfo.GetCompilationUnit(memberDeclarations, ClassDeclaration.BaseList);
            }
        }
    }
}
