// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>All of the descriptors of this library.</summary>
public static class Descriptors
{
    /// <summary>Gets the descriptor describing that the argument failed to pass the regex.</summary>
    public static DiagnosticDescriptor Eam001 { get; } = Make(
        1,
        DiagnosticSeverity.Error,
        "Argument fails regex test",
        "The argument does not match the regex specified by the parameter.",
        "The argument does not match the regex specified by the parameter: <code>{0}</code>"
    );

    /// <summary>Gets the descriptor describing that the argument is not a constant.</summary>
    public static DiagnosticDescriptor Eam002 { get; } =
        Make(
            2,
            DiagnosticSeverity.Warning,
            "Non-constant argument may fail regex test",
            "The argument may not match the regex specified by the parameter.",
            "The argument may not match the regex specified by the parameter: <code>{0}</code>"
        );

    /// <summary>Gets the descriptor describing that the regex took to long to process.</summary>
    public static DiagnosticDescriptor Eam003 { get; } =
        Make(
            3,
            DiagnosticSeverity.Info,
            "Argument regex test took too long",
            "The argument took too long to validate on whether it matches the regex specified by the parameter.",
            "The argument took too long to validate on whether it matches the regex specified by the parameter: <code>{0}</code>"
        );

    /// <summary>Gets the descriptor describing that this capture group cannot exist.</summary>
    public static DiagnosticDescriptor Eam004 { get; } =
        Make(
            4,
            DiagnosticSeverity.Error,
            "Capture group doesn't exist",
            $"This will cause an {nameof(ArgumentOutOfRangeException)} at runtime.",
            $"This will cause an {nameof(ArgumentOutOfRangeException)} at runtime."
        );

    /// <summary>Creates a diagnostic from a <see cref="RegexStatus"/>.</summary>
    /// <param name="status">The status to convert.</param>
    /// <exception cref="ArgumentOutOfRangeException">A non-valid member is passed in.</exception>
    /// <returns>The equivalent <see cref="DiagnosticDescriptor"/> of <paramref name="status"/>.</returns>
    public static DiagnosticDescriptor? From(RegexStatus status) =>
        status switch
        {
            RegexStatus.Passed => null,
            RegexStatus.Failed => Eam001,
            RegexStatus.Invalid => Eam002,
            RegexStatus.Timeout => Eam003,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
        };

    static DiagnosticDescriptor Make(
        byte id,
        DiagnosticSeverity severity,
        string title,
        string description,
        string messageFormat
    )
    {
        var index = id.ToString().PadLeft(3, '0');

        return new(
            $"EAM{index}",
            title,
            messageFormat,
            typeof(MatchAnalyzer).Namespace,
            severity,
            true,
            description,
            $"https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM{index}.md"
        );
    }
}
