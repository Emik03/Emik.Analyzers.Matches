// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <inheritdoc />
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
public sealed class MatchAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => Descriptors.Diagnostics;

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(Analyze | ReportDiagnostics);
        context.RegisterSyntaxNodeAction<InvocationExpressionSyntax>(Go, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction<ElementAccessExpressionSyntax>(Go, SyntaxKind.ElementAccessExpression);
        context.RegisterSyntaxNodeAction<ObjectCreationExpressionSyntax>(Go, SyntaxKind.ObjectCreationExpression);
    }

    static void Go(SyntaxNodeAnalysisContext context, ElementAccessExpressionSyntax arg) =>
        Go(context, arg.ArgumentList, arg);

    static void Go(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax arg) =>
        Go(context, arg.ArgumentList, arg);

    static void Go(SyntaxNodeAnalysisContext context, ObjectCreationExpressionSyntax arg) =>
        Go(context, arg.ArgumentList, arg);

    static void Go(SyntaxNodeAnalysisContext context, BaseArgumentListSyntax? argumentList, ExpressionSyntax expression)
    {
        if (argumentList is null)
            return;

        var (model, token) = (context.SemanticModel, context.CancellationToken);

        (string Pattern, Location Location)? LocateMismatches(AttributeData? x, int i) =>
            x?.ConstructorArguments is [{ Value: string pattern }, { Value: int options }] &&
            argumentList.Arguments.Nth(i) is { } a &&
            $"{model.GetConstantValue(a.Expression, token).Value}" is var input &&
            !IsMatch(input, pattern, options)
                ? (pattern, a.GetLocation())
                : null;

        Parameters(expression, model, token)
           .Select(x => x.GetAttributes().FirstOrDefault(x => x.AttributeClass?.Name is "MatchAttribute"))
           .Select(LocateMismatches)
           .Filter()
           .Select(x => Diagnostic.Create(Descriptors.Eam001, x.Location, x.Pattern).Peek(context.ReportDiagnostic))
           .Enumerate();
    }

    static bool IsMatch(string input, string pattern, int options)
    {
        try
        {
            return Regex.IsMatch(input, pattern, (RegexOptions)options);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    static ImmutableArray<IParameterSymbol> Parameters(
        ExpressionSyntax expression,
        SemanticModel model,
        CancellationToken token
    ) =>
        model.GetSymbolInfo(expression, token).Symbol switch
        {
            IMethodSymbol m => m.Parameters,
            IPropertySymbol p => p.Parameters,
            _ => ImmutableArray<IParameterSymbol>.Empty,
        };
}
