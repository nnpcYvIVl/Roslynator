﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.CodeFixes;
using Xunit;

#pragma warning disable RCS1090

namespace Roslynator.CSharp.Analysis.Tests
{
    public class RCS1206UseConditionalAccessInsteadOfConditionalExpressionTests : AbstractCSharpCodeFixVerifier
    {
        public override DiagnosticDescriptor Descriptor { get; } = DiagnosticDescriptors.UseConditionalAccessInsteadOfConditionalExpression;

        public override DiagnosticAnalyzer Analyzer { get; } = new SimplifyNullCheckAnalyzer();

        public override CodeFixProvider FixProvider { get; } = new ConditionalExpressionCodeFixProvider();

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        [InlineData("(x != null) ? x.ToString() : null", "x?.ToString()")]
        [InlineData("(x != null) ? x.ToString() : default", "x?.ToString()")]
        [InlineData("(x != null) ? x.ToString() : default(string)", "x?.ToString()")]

        [InlineData("(x == null) ? null : x.ToString()", "x?.ToString()")]
        [InlineData("(x == null) ? default : x.ToString()", "x?.ToString()")]
        [InlineData("(x == null) ? default(string) : x.ToString()", "x?.ToString()")]
        public async Task Test_ReferenceTypeToReferenceType(string fromData, string toData)
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    void M()
    {
        var x = new Foo();

        string s = [||];
    }
}
", fromData, toData);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        [InlineData("(x != null) ? x.Value : 0", "x?.Value ?? 0")]
        [InlineData("(x != null) ? x.Value : default", "x?.Value ?? (default)")]
        [InlineData("(x != null) ? x.Value : default(int)", "x?.Value ?? default(int)")]

        [InlineData("(x == null) ? 0 : x.Value", "x?.Value ?? 0")]
        [InlineData("(x == null) ? default : x.Value", "x?.Value ?? (default)")]
        [InlineData("(x == null) ? default(int) : x.Value", "x?.Value ?? default(int)")]
        public async Task Test_ReferenceTypeToValueType(string fromData, string toData)
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    void M()
    {
        var x = new Foo();

        int i = [||];
    }

    public int Value { get; }
}
", fromData, toData);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        [InlineData("(x != null) ? x.Value : null", "x?.Value")]
        [InlineData("(x != null) ? x.Value : default", "x?.Value")]
        [InlineData("(x != null) ? x.Value : default(int?)", "x?.Value")]

        [InlineData("(x == null) ? null : x.Value", "x?.Value")]
        [InlineData("(x == null) ? default : x.Value", "x?.Value")]
        [InlineData("(x == null) ? default(int?) : x.Value", "x?.Value")]
        public async Task Test_ReferenceTypeToNullableType(string fromData, string toData)
        {
            await VerifyDiagnosticAndFixAsync(@"
class Foo
{
    void M()
    {
        Foo x = null;

        int? ni = [||];
    }

    public int? Value { get; }
}
", fromData, toData);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        [InlineData("(ni != null) ? ni.Value.ToString() : null", "ni?.ToString()")]
        [InlineData("(ni == null) ? null : ni.Value.ToString()", "ni?.ToString()")]
        [InlineData("(ni.HasValue) ? ni.Value.ToString() : null", "ni?.ToString()")]
        [InlineData("(!ni.HasValue) ? null : ni.Value.ToString()", "ni?.ToString()")]
        public async Task Test_NullableTypeToReferenceType(string fromData, string toData)
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int? ni = null;

        string s = [||];
    }
}
", fromData, toData);
        }

        [Theory, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        [InlineData("(ni != null) ? ni.Value.GetHashCode() : 0", "ni?.GetHashCode() ?? 0")]
        [InlineData("(ni == null) ? 0 : ni.Value.GetHashCode()", "ni?.GetHashCode() ?? 0")]
        [InlineData("(ni.HasValue) ? ni.Value.GetHashCode() : 0", "ni?.GetHashCode() ?? 0")]
        [InlineData("(!ni.HasValue) ? 0 : ni.Value.GetHashCode()", "ni?.GetHashCode() ?? 0")]
        public async Task Test_NullableTypeToValueType(string fromData, string toData)
        {
            await VerifyDiagnosticAndFixAsync(@"
class C
{
    void M()
    {
        int? ni = null;

        int i = [||];
    }
}
", fromData, toData);
        }

        [Fact, Trait(Traits.Analyzer, DiagnosticIdentifiers.UseConditionalAccessInsteadOfConditionalExpression)]
        public async Task TestNoDiagnostic()
        {
            await VerifyNoDiagnosticAsync(@"

class Foo
{
    void M()
    {
        var x = new Foo();
        var x2 = new Foo();

        string s = null;
        int i = 0;
        int? ni = null;
        int? ni2 = null;

        i = (x != null) ? x2.Value : default(int);
        i = (x == null) ? default(int) : x2.Value;

        i = (x != null) ? x.Value : 1;
        i = (x == null) ? 1 : x.Value;

        i = (ni != null) ? ni2.Value : default(int);
        i = (ni == null) ? default(int) : ni2.Value;

        i = (ni.HasValue) ? ni2.Value : default(int);
        i = (!ni.HasValue) ? default(int) : ni2.Value;

        i = (ni != null) ? ni.Value : 1;
        i = (ni == null) ? 1 : ni.Value;

        i = (ni.HasValue) ? ni.Value : 1;
        i = (!ni.HasValue) ? 1 : ni.Value;

#pragma warning disable CS0472
        s = (i != null) ? i.ToString() : null;
        s = (i == null) ? null : i.ToString();
#pragma warning restore CS0472
    }

    public int Value { get; }
}
");
        }
    }
}
