// <copyright file="ZenGeneratorIncremental.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generators
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Text;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Generator for Zen methods for a given type.
    /// </summary>
    [Generator]
    public class ZenIncrementalGenerator : IIncrementalGenerator
    {
        /// <summary>
        /// Initialize the incremental source code generator.
        /// </summary>
        /// <param name="context">The incremental generator initialization context.</param>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var objectDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (s, _) => IsSyntaxTarget(s),
                    transform: static (ctx, _) => GetClassForGeneration(ctx))
                .Where(static m => m is not null);

            var compilationAndClasses = context.CompilationProvider.Combine(objectDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses, static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        /// <summary>
        /// Determine if a syntax node is a target for generation.
        /// </summary>
        /// <param name="syntaxNode">The syntax node.</param>
        /// <returns>True or false.</returns>
        internal static bool IsSyntaxTarget(SyntaxNode syntaxNode)
        {
            return (syntaxNode is ClassDeclarationSyntax c && c.AttributeLists.Count > 0) || (syntaxNode is StructDeclarationSyntax s && s.AttributeLists.Count > 0);
        }

        /// <summary>
        /// Get the class for source code generation.
        /// </summary>
        /// <param name="context">The generation syntax context.</param>
        /// <returns>The class syntax or null if it doesn't have the ZenObject attribute.</returns>
        internal static SyntaxNode GetClassForGeneration(GeneratorSyntaxContext context)
        {
            var attributeLists = context.Node is ClassDeclarationSyntax ?
                ((ClassDeclarationSyntax)context.Node).AttributeLists :
                ((StructDeclarationSyntax)context.Node).AttributeLists;

            foreach (var attributeListSyntax in attributeLists)
            {
                foreach (var attributeSyntax in attributeListSyntax.Attributes)
                {
                    var attributeSymbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                    if (attributeSymbol == null)
                    {
                        continue;
                    }

                    if (attributeSymbol.ContainingType.ToDisplayString() == "ZenLib.ZenObjectAttribute")
                    {
                        return context.Node;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Execute the source code generator for the classes.
        /// </summary>
        /// <param name="compilation">The compilation object.</param>
        /// <param name="syntaxNodes">The nodes with the ZenObject attribute.</param>
        /// <param name="context">The source code production context.</param>
        private static void Execute(Compilation compilation, ImmutableArray<SyntaxNode> syntaxNodes, SourceProductionContext context)
        {
            if (syntaxNodes.IsDefaultOrEmpty)
            {
                return;
            }

            var distinctClasses = syntaxNodes.Distinct();

            foreach (var syntaxNode in distinctClasses)
            {
                var namedTypedSymbol = compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetDeclaredSymbol(syntaxNode) as INamedTypeSymbol;
                var sourceCodeText = SourceText.From(CreateZenCode(namedTypedSymbol), Encoding.UTF8);
                context.AddSource($"{namedTypedSymbol.Name}Extensions_{namedTypedSymbol.Arity}_zen_yo.cs", sourceCodeText);
            }
        }

        /// <summary>
        /// Create the Zen code for the class.
        /// </summary>
        /// <param name="typeSymbol">The class.</param>B
        /// <returns>The text of the new class to compile.</returns>
        private static string CreateZenCode(INamedTypeSymbol typeSymbol)
        {
            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            var allProperties = GetPropertySymbols(typeSymbol).ToArray();
            var allFields = GetFieldSymbols(typeSymbol).ToArray();
            var typeString = typeSymbol.ToDisplayString();
            var accessModifier = typeSymbol.DeclaredAccessibility.ToString().ToLowerInvariant();
            var genericTypes = typeSymbol.Arity == 0 ?
                string.Empty :
                "<" + string.Join(",", typeSymbol.TypeParameters.Select(x => x.ToDisplayString())) + ">";

            var sb = new StringBuilder();

            sb.AppendLine($"// -------------------------------");
            sb.AppendLine($"// THIS FILE IS AUTOGENERATED");
            sb.AppendLine($"// -------------------------------");
            sb.AppendLine();
            sb.AppendLine($"namespace {namespaceName}");
            sb.AppendLine($"{{");
            sb.AppendLine($"    using ZenLib;");
            sb.AppendLine();
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Extension methods for Zen objects of type {typeSymbol.Name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    {accessModifier} static class {typeSymbol.Name}Extensions_zen");
            sb.AppendLine($"    {{");

            // add get methods to access fields
            foreach (var property in allProperties)
            {
                var propertyTypeString = property.Type.ToDisplayString();

                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Get the {property.Name} field for the {typeString} type.");
                sb.AppendLine($"        /// </summary>");
                sb.AppendLine($"        /// <param name=\"x\">The Zen object.</param>");
                sb.AppendLine($"        /// /// <returns>The {property.Name} field as a Zen value.</returns>");
                sb.AppendLine($"        public static Zen<{propertyTypeString}> Get{property.Name}{genericTypes}(this Zen<{typeString}> x)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return x.GetField<{typeString}, {propertyTypeString}>(\"{property.Name}\");");
                sb.AppendLine($"        }}");
                sb.AppendLine();
            }

            foreach (var field in allFields)
            {
                var fieldTypeString = field.Type.ToDisplayString();

                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Get the {field.Name} field for the {typeString} type.");
                sb.AppendLine($"        /// </summary>");
                sb.AppendLine($"        /// <param name=\"x\">The Zen object.</param>");
                sb.AppendLine($"        /// /// <returns>The {field.Name} field as a Zen value.</returns>");
                sb.AppendLine($"        public static Zen<{fieldTypeString}> Get{field.Name}{genericTypes}(this Zen<{typeString}> x)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return x.GetField<{typeString}, {fieldTypeString}>(\"{field.Name}\");");
                sb.AppendLine($"        }}");
                sb.AppendLine();
            }

            // add with methods to update fields
            foreach (var property in allProperties)
            {
                var propertyTypeString = property.Type.ToDisplayString();

                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Update the {property.Name} field for the {typeString} type.");
                sb.AppendLine($"        /// </summary>");
                sb.AppendLine($"        /// <param name=\"x\">The Zen object.</param>");
                sb.AppendLine($"        /// <param name=\"field\">The new Zen field value.</param>");
                sb.AppendLine($"        /// /// <returns>The {property.Name} field as a Zen value.</returns>");
                sb.AppendLine($"        public static Zen<{typeString}> With{property.Name}{genericTypes}(this Zen<{typeString}> x, Zen<{propertyTypeString}> field)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return x.WithField<{typeString}, {propertyTypeString}>(\"{property.Name}\", field);");
                sb.AppendLine($"        }}");
                sb.AppendLine();
            }

            foreach (var field in allFields)
            {
                var fieldTypeString = field.Type.ToDisplayString();

                sb.AppendLine($"        /// <summary>");
                sb.AppendLine($"        /// Update the {field.Name} field for the {typeString} type.");
                sb.AppendLine($"        /// </summary>");
                sb.AppendLine($"        /// <param name=\"x\">The Zen object.</param>");
                sb.AppendLine($"        /// <param name=\"field\">The new Zen field value.</param>");
                sb.AppendLine($"        /// /// <returns>The {field.Name} field as a Zen value.</returns>");
                sb.AppendLine($"        public static Zen<{typeString}> With{field.Name}{genericTypes}(this Zen<{typeString}> x, Zen<{fieldTypeString}> field)");
                sb.AppendLine($"        {{");
                sb.AppendLine($"            return x.WithField<{typeString}, {fieldTypeString}>(\"{field.Name}\", field);");
                sb.AppendLine($"        }}");
                sb.AppendLine();
            }

            sb.AppendLine($"    }}");
            sb.AppendLine($"}}");

            return sb.ToString();
        }

        /// <summary>
        /// Get the properties for the given class.
        /// </summary>
        /// <param name="classSymbol">The class symbol.</param>
        /// <returns>The property symbols.</returns>
        private static IEnumerable<IPropertySymbol> GetPropertySymbols(ITypeSymbol classSymbol)
        {
            return classSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(x => x.SetMethod != null && x.GetMethod != null && x.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName);
        }

        /// <summary>
        /// Get the fields for the given class.
        /// </summary>
        /// <param name="classSymbol">The class symbol.</param>
        /// <returns>The field symbols.</returns>
        private static IEnumerable<IFieldSymbol> GetFieldSymbols(ITypeSymbol classSymbol)
        {
            return classSymbol
                .GetMembers()
                .OfType<IFieldSymbol>()
                .Where(x => !x.IsStatic && x.DeclaredAccessibility == Accessibility.Public && x.CanBeReferencedByName);
        }
    }
}