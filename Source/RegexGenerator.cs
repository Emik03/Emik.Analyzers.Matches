// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>Generates the extension methods that are later analyzed when used.</summary>
[Generator]
public sealed class RegexGenerator : ISourceGenerator
{
    /// <summary>The name of the extension method added by this generator.</summary>
    public const string TypeName = "RegexDeconstructors";

    /// <summary>The name of the extension method added by this generator.</summary>
    public const string MethodName = nameof(Regex.IsMatch);

    /// <summary>Gets the contents to generate a source of.</summary>
    public static string Contents { get; } = Methods();

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context) =>
        context.AddSource($"Emik.{TypeName}.g.cs", Contents);

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context) { }

    static string Methods() =>
        CSharp(
            0,
            $$"""
            // <auto-generated/>
            // ReSharper disable RedundantNameQualifier
            // ReSharper disable once CheckNamespace
            #nullable enable
            namespace Emik
            {
                /// <summary>Declares a contract that the generic parameter must include the qualified member.</summary>
                internal static class {{TypeName}}
                {
            {{(1..15).For().Select(Method).Conjoin("\n\n")}}
                }
            }
            """
        );

    static string Method(int arg) =>
        CSharp(
            2,
            $$"""
            public static bool {{MethodName}}(
                this global::System.Text.RegularExpressions.Regex regex,
                [global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] string? input,
            {{Parameter(arg)}}
            )
            {
            {{InitializeParameters(arg)}}

                if (input == null)
                    return false;

                var match = regex.Match(input);

                if (!match.Success)
                    return false;

                global::System.Diagnostics.Debug.Assert(match.Groups.Count == {{arg}}, "Group count must be {{arg}}.");

            {{SetParametersToGroups(arg)}}

                return true;
            }
            """
        );

    static string Parameter(int length) =>
        length
           .For(
                x => CSharp(
                    1,
                    $"[global::System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out global::System.Text.RegularExpressions.Group n{x}"
                )
            )
           .Conjoin(",\n");

    static string InitializeParameters(int length) => length.For(x => CSharp(1, $"n{x} = default;")).Conjoin("\n");

    static string SetParametersToGroups(int length) =>
        length.For(x => CSharp(1, $"n{x} = match.Groups[{x}];")).Conjoin("\n");

    static string CSharp(int indent, [StringSyntax("C#")] string code) =>
        code
           .Split('\n')
           .Select(x => $"{new string(' ', indent * 4)}{x}")
           .Conjoin('\n');
}
