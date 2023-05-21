// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>Generates the attribute needed to use this analyzer.</summary>
[Generator]
public sealed class MatchGenerator : ISourceGenerator
{
    /// <summary>Initializes a new instance of the <see cref="MatchGenerator"/> class.</summary>
    public MatchGenerator()
        : this(
            """
            // <auto-generated/>
            // ReSharper disable RedundantNameQualifier
            // ReSharper disable once CheckNamespace
            #nullable enable
            namespace Emik
            {
                /// <summary>Declares a contract that the generic parameter must include the qualified member.</summary>
                [global::System.AttributeUsage(global::System.AttributeTargets.Parameter)]
                internal sealed class MatchAttribute : global::System.Attribute
                {
                    /// <summary>Initializes a new instance of the <see cref="Emik.MatchAttribute"/> class.</summary>
                    /// <param name="pattern">The regular expression pattern to match.</param>
                    /// <param name="options">The bitwise combination of the enumeration values that modify the regular expression.</param>
                    public MatchAttribute([global::System.Diagnostics.CodeAnalysis.StringSyntax(global::System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.Regex)] string pattern, global::System.Text.RegularExpressions.RegexOptions options = global::System.Text.RegularExpressions.RegexOptions.None)
                    {
                        Pattern = pattern;
                        Options = options;
                    }

                    /// <summary>Gets the regular expression to match.</summary>
                    [global::System.Diagnostics.CodeAnalysis.StringSyntax(global::System.Diagnostics.CodeAnalysis.StringSyntaxAttribute.Regex)]
                    public string Pattern { get; }

                    /// <summary>Gets the bitwise combination of the enumeration values that modify the regular expression.</summary>
                    public global::System.Text.RegularExpressions.RegexOptions Options { get; }
                }
            }
            """
        ) { }

    /// <summary>Initializes a new instance of the <see cref="MatchGenerator"/> class with a specified source.</summary>
    /// <param name="contents">The contents of the source.</param>
    public MatchGenerator([StringSyntax("C#")] string contents) => Contents = contents;

    /// <summary>Gets the contents to generate a source of.</summary>
    public string Contents { get; }

    /// <inheritdoc />
    public void Execute(GeneratorExecutionContext context) => context.AddSource("Emik.MatchAttribute.g.cs", Contents);

    /// <inheritdoc />
    public void Initialize(GeneratorInitializationContext context) { }
}
