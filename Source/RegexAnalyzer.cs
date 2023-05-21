// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <inheritdoc />
// ReSharper disable MissingIndent WrongIndentSize
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
public sealed class RegexAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.Eam004);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(Analyze | ReportDiagnostics);
        context.RegisterSyntaxNodeAction<InvocationExpressionSyntax>(Go, SyntaxKind.InvocationExpression);
    }

    static void Go(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax arg)
    {
        var (token, model) = (context.CancellationToken, context.SemanticModel);

        if (TryGetRegexDeconstructMethod(arg, model, token) is not { } method)
            return;

        if (!IsFromRegexGenerator(method.Parameters.GetEnumerator()))
            return;

        var regex = FromSymbolWithGeneratedRegex(arg, model, token);
        regex ??= FromObjectCreation(arg, model, token);
        regex ??= FromField(arg, model, token);

        var requiredNumberOfGroups = regex?.GetGroupNumbers().Length;
        var numberOfGroups = method.Parameters.Count(IsGroup);

        if (requiredNumberOfGroups == numberOfGroups)
            return;

        Diagnostic
           .Create(Descriptors.Eam004, arg.GetLocation())
           .Peek(context.ReportDiagnostic);
    }

    static IMethodSymbol? TryGetRegexDeconstructMethod(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        model.GetSymbolInfo(syntax.Expression, token).Symbol is IMethodSymbol
        {
            ContainingNamespace.Name: nameof(Emik), Name: nameof(Match), ReturnType.Name: nameof(Boolean),
        } method
            ? method
            : null;

    static Regex? FromConstructor(
        ObjectCreationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        model.GetSymbolInfo(syntax.Type, token).Symbol is ITypeSymbol { Name: nameof(Regex) } &&
        syntax.ArgumentList?.Arguments.ToArray() is [var argument, .. var rest] &&
        model.GetConstantValue(argument.Expression, token) is { HasValue: true, Value: string pattern } &&
        GetConstantValue(model, rest) is var options
            ? RegexCache.Get(pattern, options)
            : null;

    static Regex? FromObjectCreation(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        syntax.Expression is MemberAccessExpressionSyntax { Expression: ObjectCreationExpressionSyntax creation }
            ? FromConstructor(creation, model, token)
            : null;

    static Regex? FromSymbolWithGeneratedRegex(
        InvocationExpressionSyntax arg,
        SemanticModel model,
        CancellationToken token
    ) =>
        arg.Expression is MemberAccessExpressionSyntax
        {
            Expression: InvocationExpressionSyntax { Expression: var reference },
        } &&
        model.GetSymbolInfo(reference, token).Symbol is IMethodSymbol candidate &&
        candidate
           .GetAttributes()
           .SingleOrDefault(IsGeneratedRegex) is { ConstructorArguments: [{ Value: string pattern }, .. var rest] }
            ? RegexCache.Get(pattern, rest.FirstOrDefault().Value as RegexOptions? ?? RegexOptions.None)
            : null;

    static RegexOptions GetConstantValue(SemanticModel model, IEnumerable<ArgumentSyntax>? rest) =>
        rest?.FirstOrDefault() is { } options &&
        model.GetConstantValue(options.Expression) is { HasValue: true, Value: int constant }
            ? (RegexOptions)constant
            : RegexOptions.None;

    static bool IsFromRegexGenerator(ImmutableArray<IParameterSymbol>.Enumerator args)
    {
        if (!args.MoveNext())
            return false;

        if (args.Current is { RefKind: RefKind.None, Type.Name: nameof(Regex) } && !args.MoveNext())
            return false;

        if (args.Current is not { RefKind: RefKind.None, Type.Name: nameof(String) } || !args.MoveNext())
            return false;

        do
            if (!IsGroup(args.Current))
                return false;
        while (args.MoveNext());

        return true;
    }

    static bool IsGeneratedRegex(AttributeData x) => x.AttributeClass?.Name is nameof(GeneratedRegexAttribute);

    static bool IsGroup(IParameterSymbol parameter) => parameter is { RefKind: RefKind.Out, Type.Name: nameof(Group) };
}
