// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>All of the descriptors of this library.</summary>
public static class Descriptors
{
    static int s_id;

    /// <summary>Gets the descriptor describing that the argument failed to pass the regex.</summary>
    public static DiagnosticDescriptor Eam001 { get; } = Make(
        DiagnosticSeverity.Error,
        "Argument fails regex test",
        "The argument does not match the regex specified by the parameter.",
        "The argument does not match the regex specified by the parameter: <code>{0}</code>"
    );

    /// <summary>Gets the descriptor describing that the argument is not a constant.</summary>
    public static DiagnosticDescriptor Eam002 { get; } =
        Make(
            DiagnosticSeverity.Warning,
            "Non-constant argument may fail regex test",
            "The argument may not match the regex specified by the parameter.",
            "The argument may not match the regex specified by the parameter: <code>{0}</code>"
        );

    /// <summary>Gets the descriptor describing that the regex took to long to process.</summary>
    public static DiagnosticDescriptor Eam003 { get; } =
        Make(
            DiagnosticSeverity.Info,
            "Argument regex test took too long",
            "The argument took too long to validate on whether it matches the regex specified by the parameter.",
            "The argument took too long to validate on whether it matches the regex specified by the parameter: <code>{0}</code>"
        );

    /// <summary>Gets the descriptor describing that this capture group cannot exist.</summary>
    public static DiagnosticDescriptor Eam004 { get; } =
        Make(
            DiagnosticSeverity.Error,
            "Capture group doesn't exist",
            $"This will cause an {nameof(ArgumentOutOfRangeException)} at runtime.",
            $"This will cause an {nameof(ArgumentOutOfRangeException)} at runtime. Use the overload with {{0}} out parameters."
        );

    /// <summary>Gets the descriptor describing that this capture group cannot exist.</summary>
    public static DiagnosticDescriptor Eam005 { get; } =
        Make(
            DiagnosticSeverity.Warning,
            "Non-constant argument may have wrong number of capture groups",
            $"This may cause an {nameof(ArgumentOutOfRangeException)} at runtime.",
            $"This may cause an {nameof(ArgumentOutOfRangeException)} at runtime. Consider referencing a pre-determinable {nameof(Regex)} object."
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

    /// <summary>Creates a diagnostic from two lengths.</summary>
    /// <param name="expectedNumberOfGroups">The expected amount of groups, if applicable.</param>
    /// <param name="actualNumberOfGroups">The actual amount of groups.</param>
    /// <returns>The equivalent <see cref="DiagnosticDescriptor"/> from the arguments.</returns>
    public static DiagnosticDescriptor? From(int? expectedNumberOfGroups, int actualNumberOfGroups) =>
        expectedNumberOfGroups switch
        {
            not null when expectedNumberOfGroups == actualNumberOfGroups => null,
            not null => Eam004,
            _ => Eam005,
        };

    static DiagnosticDescriptor Make(
        DiagnosticSeverity severity,
        string title,
        string description,
        string messageFormat
    )
    {
        var index = (++s_id).ToString().PadLeft(3, '0');

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
