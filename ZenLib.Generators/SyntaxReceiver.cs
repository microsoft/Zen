// <copyright file="ZenException.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generators
{
    using System.Collections.Generic;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// A syntax receiver to fetch relevant classes.
    /// </summary>
    internal class SyntaxReceiver : ISyntaxReceiver
    {
        /// <summary>
        /// The classes that are annotated for generation.
        /// </summary>
        public IList<ClassDeclarationSyntax> ClassesWithAttributes { get; } = new List<ClassDeclarationSyntax>();

        /// <summary>
        /// The structs that are annotated for generation.
        /// </summary>
        public IList<StructDeclarationSyntax> StructsWithAttributes { get; } = new List<StructDeclarationSyntax>();

        /// <summary>
        /// Callback for every syntax node in the source code.
        /// </summary>
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is StructDeclarationSyntax structDeclarationSyntax && structDeclarationSyntax.AttributeLists.Count > 0)
            {
                this.StructsWithAttributes.Add(structDeclarationSyntax);
            }

            if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax && classDeclarationSyntax.AttributeLists.Count > 0)
            {
                this.ClassesWithAttributes.Add(classDeclarationSyntax);
            }
        }
    }
}
