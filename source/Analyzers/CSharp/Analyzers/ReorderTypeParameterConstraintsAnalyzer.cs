﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Syntax;

namespace Roslynator.CSharp.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReorderTypeParameterConstraintsAnalyzer : BaseDiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get { return ImmutableArray.Create(DiagnosticDescriptors.ReorderTypeParameterConstraints); }
        }

        public override void Initialize(AnalysisContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            base.Initialize(context);

            context.RegisterSyntaxNodeAction(AnalyzeTypeParameterList, SyntaxKind.TypeParameterList);
        }

        public static void AnalyzeTypeParameterList(SyntaxNodeAnalysisContext context)
        {
            var typeParameterList = (TypeParameterListSyntax)context.Node;

            GenericInfo genericInfo = SyntaxInfo.GenericInfo(typeParameterList);

            if (!genericInfo.Success)
                return;

            if (!genericInfo.TypeParameters.Any())
                return;

            if (!genericInfo.ConstraintClauses.Any())
                return;

            if (genericInfo.ConstraintClauses.SpanContainsDirectives())
                return;

            if (!IsFixable(genericInfo.TypeParameters, genericInfo.ConstraintClauses))
                return;

            context.ReportDiagnostic(
                DiagnosticDescriptors.ReorderTypeParameterConstraints,
                genericInfo.ConstraintClauses.First());
        }

        private static bool IsFixable(
            SeparatedSyntaxList<TypeParameterSyntax> typeParameters,
            SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses)
        {
            int lastIndex = -1;

            for (int i = 0; i < typeParameters.Count; i++)
            {
                string name = typeParameters[i].Identifier.ValueText;

                int index = IndexOf(constraintClauses, name);

                if (index != -1)
                {
                    if (index < lastIndex)
                        return true;

                    lastIndex = index;
                }
            }

            return false;
        }

        internal static int IndexOf(SyntaxList<TypeParameterConstraintClauseSyntax> constraintClauses, string name)
        {
            for (int i = 0; i < constraintClauses.Count; i++)
            {
                if (constraintClauses[i].Name.Identifier.ValueText == name)
                    return i;
            }

            return -1;
        }
    }
}