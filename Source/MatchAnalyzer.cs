// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <inheritdoc />
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
public sealed class MatchAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.Eam001, Descriptors.Eam002, Descriptors.Eam003);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.ConfigureGeneratedCodeAnalysis(Analyze | ReportDiagnostics);

        context
           .RegisterSyntaxNodeAction<VariableDeclaratorSyntax>(Go, VariableDeclarator)
           .RegisterSyntaxNodeAction<ElementAccessExpressionSyntax>(Go, ElementAccessExpression)
           .RegisterSyntaxNodeAction<ObjectCreationExpressionSyntax>(Go, ObjectCreationExpression)
           .RegisterSyntaxNodeAction<InvocationExpressionSyntax>(Go, SyntaxKind.InvocationExpression)
           .RegisterSyntaxNodeAction<PrimaryConstructorBaseTypeSyntax>(Go, PrimaryConstructorBaseType)
           .RegisterSyntaxNodeAction<ImplicitObjectCreationExpressionSyntax>(Go, ImplicitObjectCreationExpression);
    }

    static void Go(SyntaxNodeAnalysisContext context, VariableDeclaratorSyntax _) => Go(context, argumentList: null);

    static void Go(SyntaxNodeAnalysisContext context, BaseObjectCreationExpressionSyntax arg) =>
        Go(context, arg.ArgumentList);

    static void Go(SyntaxNodeAnalysisContext context, ElementAccessExpressionSyntax arg) =>
        Go(context, arg.ArgumentList);

    static void Go(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax arg) => Go(context, arg.ArgumentList);

    static void Go(SyntaxNodeAnalysisContext context, PrimaryConstructorBaseTypeSyntax arg) =>
        Go(context, arg.ArgumentList);

    static void Go(SyntaxNodeAnalysisContext context, BaseArgumentListSyntax? argumentList)
    {
        var (token, node, model) = (context.CancellationToken, context.Node, context.SemanticModel);

        (node switch
            {
                ExpressionSyntax expression => Parameters(expression, model, token)
                   .Select(GetAttributes)
                   .Select(ExpressionFromArgumentList)
                   .TakeWhile(x => x.Syntax is not null)
                   .Select(x => LocateMismatches(x.Data, x.Syntax)),
                PrimaryConstructorBaseTypeSyntax primary => Parameters(primary, model, token)
                   .Select(GetAttributes)
                   .Select(ExpressionFromArgumentList)
                   .TakeWhile(x => x.Syntax is not null)
                   .Select(x => LocateMismatches(x.Data, x.Syntax)),
                VariableDeclaratorSyntax
                    {
                        Initializer.Value: var value, Parent: VariableDeclarationSyntax { Type: var type },
                    } when model.GetTypeInfo(value, token).Type?.Name is nameof(String) &&
                    model.GetTypeInfo(type, token).Type is { } symbol => GetAttributes(symbol)
                       .Select(x => LocateMismatches(x, value)),
                _ => Enumerable.Empty<Diagnostic>(),
            })
           .Filter()
           .Lazily(context.ReportDiagnostic)
           .Enumerate();

        (AttributeData? Data, ExpressionSyntax? Syntax) ExpressionFromArgumentList(AttributeData? data, int index) =>
            (data, argumentList?.Arguments.Nth(index)?.Expression);

        Diagnostic? LocateMismatches(AttributeData? data, SyntaxNode? node) =>
            node is not null &&
            data?.ConstructorArguments is [{ Value: string pattern }, { Value: int options }] &&
            model.GetConstantValue(node, token) is var constant &&
            Match(constant, pattern, options) is var match &&
            Descriptors.From(match) is { } descriptor
                ? Diagnostic.Create(descriptor, node.GetLocation(), pattern)
                : null;
    }

    static AttributeData? GetAttributes(IParameterSymbol parameter) =>
        parameter
           .GetAttributes()
           .AddRange(GetAttributes(parameter.Type))
           .FirstOrDefault(x => x.AttributeClass?.Name is MatchGenerator.TypeName);

    static IEnumerable<AttributeData> GetAttributes(ITypeSymbol type) =>
        type.TryFindSingleMethod(IsConversion, out var conversion)
            ? conversion.Parameters.SelectMany(x => x.GetAttributes())
            : Enumerable.Empty<AttributeData>();

    static bool IsConversion(IMethodSymbol x) =>
        x is { MethodKind: MethodKind.Conversion, Parameters: [{ Type.Name: nameof(String) }] };

    static RegexStatus Match(Optional<object?> constant, string pattern, int options)
    {
        if (constant is not { HasValue: true, Value: var input })
            return RegexStatus.Invalid;

        try
        {
            return RegexCache.Get(pattern, (RegexOptions)options) is { } regex && !regex.IsMatch($"{input}")
                ? RegexStatus.Failed
                : RegexStatus.Passed;
        }
        catch (RegexMatchTimeoutException)
        {
            return RegexStatus.Timeout;
        }
    }

    static IEnumerable<IParameterSymbol> Parameters(
        ExpressionSyntax expression,
        SemanticModel model,
        CancellationToken token
    ) =>
        Parameters(model.GetSymbolInfo(expression, token).Symbol);

    static IEnumerable<IParameterSymbol> Parameters(
        PrimaryConstructorBaseTypeSyntax expression,
        SemanticModel model,
        CancellationToken token
    ) =>
        Parameters(model.GetSymbolInfo(expression, token).Symbol);

    static IEnumerable<IParameterSymbol> Parameters(ISymbol? symbol) =>
        symbol switch
        {
            IMethodSymbol m => m.Parameters,
            IPropertySymbol p => p.Parameters,
            _ => ImmutableArray<IParameterSymbol>.Empty,
        } is var parameters &&
        parameters is [.., { IsParams: true } last]
            ? parameters.Concat(last.Forever())
            : parameters;
}
