// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>
/// <see cref="AnalysisContext.RegisterSyntaxNodeAction{TLanguageKindEnum}(Action{SyntaxNodeAnalysisContext}, TLanguageKindEnum[])"/>
/// with a wrapped callback which filters out ignored contexts.
/// </summary>
static class IncludedSyntaxNodeRegistrant
{
    /// <inheritdoc cref="AttributeArgumentSyntaxExt.TryGetStringValue(AttributeArgumentSyntax, SemanticModel, CancellationToken, out string)"/>
    internal static string? StringValue(this SyntaxNodeAnalysisContext context, AttributeArgumentSyntax syntax)
    {
        syntax.TryGetStringValue(context.SemanticModel, context.CancellationToken, out var result);
        return result;
    }

    /// <inheritdoc cref="MemberPath.TryGetMemberName(ExpressionSyntax, out string)"/>
    internal static string? MemberName(this ExpressionSyntax syntax)
    {
        syntax.TryGetMemberName(out var result);
        return result;
    }

    /// <inheritdoc cref="AttributeArgumentSyntaxExt.TryGetStringValue(AttributeArgumentSyntax, SemanticModel, CancellationToken, out string)"/>
    internal static IEnumerable<ISymbol> Symbols(this SyntaxNodeAnalysisContext context, ExpressionSyntax syntax) =>
        (syntax.MemberName() ?? $"{syntax}") is var name && syntax is PredefinedTypeSyntax
            ? context.Compilation.GetSymbolsWithName(
                x => x.Contains(name),
                cancellationToken: context.CancellationToken
            )
            : context.SemanticModel.LookupSymbols(syntax.SpanStart, name: name);

    /// <inheritdoc cref="AnalysisContext.RegisterSyntaxNodeAction{TLanguageKindEnum}(Action{SyntaxNodeAnalysisContext}, TLanguageKindEnum[])"/>
    internal static void RegisterSyntaxNodeAction<TSyntaxNode>(
        this AnalysisContext context,
        Action<SyntaxNodeAnalysisContext, TSyntaxNode> action,
        params SyntaxKind[] syntaxKinds
    )
        where TSyntaxNode : SyntaxNode =>
        context.RegisterSyntaxNodeAction(Filter(action), syntaxKinds);

    /// <inheritdoc cref="AnalysisContext.RegisterSyntaxNodeAction{TLanguageKindEnum}(Action{SyntaxNodeAnalysisContext}, ImmutableArray{TLanguageKindEnum})"/>
    internal static void RegisterSyntaxNodeAction<TSyntaxNode>(
        this AnalysisContext context,
        Action<SyntaxNodeAnalysisContext, TSyntaxNode> action,
        ImmutableArray<SyntaxKind> syntaxKinds
    )
        where TSyntaxNode : SyntaxNode =>
        context.RegisterSyntaxNodeAction(Filter(action), syntaxKinds);

    /// <summary>Adds information to a diagnostic.</summary>
    /// <param name="diagnostic">The diagnostic to append.</param>
    /// <param name="message">The string to append.</param>
    /// <returns>The diagnostic with added information.</returns>
    internal static Diagnostic And(this Diagnostic diagnostic, string message) =>
        Diagnostic.Create(
            new(
                diagnostic.Descriptor.Id,
                diagnostic.Descriptor.Title,
                $"{diagnostic.Descriptor.MessageFormat} {message}",
                diagnostic.Descriptor.Category,
                diagnostic.Descriptor.DefaultSeverity,
                diagnostic.Descriptor.IsEnabledByDefault,
                $"{diagnostic.Descriptor.Description} {message}",
                diagnostic.Descriptor.HelpLinkUri,
                diagnostic.Descriptor.CustomTags.ToArray()
            ),
            diagnostic.Location,
            diagnostic.Severity,
            diagnostic.AdditionalLocations,
            diagnostic.Properties
        );

    static Action<SyntaxNodeAnalysisContext> Filter<TSyntaxNode>(Action<SyntaxNodeAnalysisContext, TSyntaxNode> action)
        where TSyntaxNode : SyntaxNode =>
        context =>
        {
            if (!context.IsExcludedFromAnalysis() && context.Node is TSyntaxNode node)
                action(context, node);
        };
}
