// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>All of the descriptors of this library.</summary>
public static class Descriptors
{
    /// <summary>
    /// Gets the descriptor describing that the argument failed to pass the regex as specified by the parameter.
    /// </summary>
    public static DiagnosticDescriptor Eam001 { get; } = new(
        "EAM001",
        "Argument fails regex",
        "This argument failed the parameter's following regex: <code>{0}</code>",
        typeof(MatchAnalyzer).Namespace,
        DiagnosticSeverity.Warning,
        true,
        "The argument failed to pass the regex as specified by the parameter.",
        "https://github.com/Emik03/Emik.Analyzers.Matches"
    );

    /// <summary>Gets all of the library's diagnostics.</summary>
    public static ImmutableArray<DiagnosticDescriptor> Diagnostics { get; } = ImmutableArray.Create(Eam001);
}
