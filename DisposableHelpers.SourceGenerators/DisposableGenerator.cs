using DisposableHelpers.SourceGenerators.ComponentModel.Models;
using DisposableHelpers.SourceGenerators.Diagnostics;
using DisposableHelpers.SourceGenerators.Extensions;
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
    public sealed partial class DisposableGenerator : TransitiveMembersGenerator<DisposableInfo>
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
                var disposeMethod = typeSymbol.GetMembers().FirstOrDefault(i =>
                    i is IMethodSymbol symbol &&
                    symbol.Name == "Dispose" &&
                    symbol.Parameters.Length == 1 &&
                    symbol.Parameters[0].Type.Name == typeof(bool).Name);

                return new(
                    typeName,
                    hasExplicitDestructors,
                    disposeMethod as IMethodSymbol);
            }

            return
                source
                .Select(static (item, _) => (item.Symbol, GetInfo(item.Symbol, item.AttributeData)));
        }

        protected override bool ValidateTargetType(INamedTypeSymbol typeSymbol, DisposableInfo info, out ImmutableArray<Diagnostic> diagnostics)
        {
            ImmutableArray<Diagnostic>.Builder builder = ImmutableArray.CreateBuilder<Diagnostic>();

            // Check if the type already implements IDisposable...
            if (typeSymbol.AllInterfaces.Any(i => i.HasFullyQualifiedName("global::System.IDisposable")))
            {
                builder.Add(DuplicateIDisposableInterfaceForDisposableAttributeError, typeSymbol, typeSymbol);

                diagnostics = builder.ToImmutable();

                return false;
            }

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

                    builder.Add((DestructorDeclarationSyntax)ParseMemberDeclaration(replaced)!);
                }
            }

            MemberDeclarationSyntax? FixupFilteredMemberDeclaration(MemberDeclarationSyntax member)
            {
                // Remove Dispose(bool) if the type already has the method
                if (info.DisposeMethod != null &&
                    member is MethodDeclarationSyntax syntax &&
                    syntax.Identifier.ValueText == "Dispose" &&
                    syntax.ParameterList.Parameters.Count == 1 &&
                    syntax.ParameterList.Parameters[0].Type?.ToString() == "bool")
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
    }
}
