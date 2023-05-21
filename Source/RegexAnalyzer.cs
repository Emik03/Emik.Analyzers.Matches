// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <inheritdoc />
// ReSharper disable MissingIndent WrongIndentSize
[DiagnosticAnalyzer(LanguageNames.CSharp, LanguageNames.FSharp, LanguageNames.VisualBasic)]
public sealed class RegexAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Descriptors.Eam004, Descriptors.Eam005);

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
        regex ??= FromReference(arg, model, token);

        var expected = regex?.GetGroupNumbers().Length;
        var actual = method.Parameters.Count(IsGroup);

        if (Descriptors.From(expected, actual) is { } descriptor)
            Diagnostic.Create(descriptor, arg.GetLocation(), expected).Peek(context.ReportDiagnostic);
    }

    static Regex? FromSymbolWithGeneratedRegex(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        Reference(syntax) is { } reference &&
        model.GetSymbolInfo(reference, token).Symbol is IMethodSymbol candidate &&
        candidate
           .GetAttributes()
           .SingleOrDefault(IsGeneratedRegex) is { ConstructorArguments: [{ Value: string pattern }, .. var rest] }
            ? RegexCache.Get(pattern, rest.FirstOrDefault().Value as RegexOptions? ?? RegexOptions.None)
            : null;

    static Regex? FromObjectCreation(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        Constructor(syntax) is { } creation &&
        model.GetSymbolInfo(creation.Type, token).Symbol is ITypeSymbol { Name: nameof(Regex) }
            ? FromConstructor(creation, model, token)
            : null;

    static Regex? FromReference(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        Identifier(syntax.Expression as MemberAccessExpressionSyntax) is { } identifier &&
        model.GetSymbolInfo(identifier, token).Symbol is { } symbol &&
        symbol.ToUnderlying()?.Name is nameof(Regex)
            ? symbol
               .DeclaringSyntaxReferences
               .Select(x => x.GetSyntax(token))
               .Select(Initializer)
               .Filter()
               .Select(x => FromConstructor(x, model, token))
               .SingleOrDefault()
            : null;

    static Regex? FromConstructor(
        BaseObjectCreationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        syntax.ArgumentList?.Arguments.ToArray() is [var argument, .. var rest] &&
        model.GetConstantValue(argument.Expression, token) is { HasValue: true, Value: string pattern } &&
        GetConstantValue(model, rest) is var options
            ? RegexCache.Get(pattern, options)
            : null;

    static ExpressionSyntax? Reference(InvocationExpressionSyntax syntax) =>
        ((syntax.Expression as MemberAccessExpressionSyntax)?.Expression as InvocationExpressionSyntax)?.Expression ??
        syntax.ArgumentList.Arguments.FirstOrDefault()?.Expression;

    static ObjectCreationExpressionSyntax? Constructor(InvocationExpressionSyntax syntax) =>
        ((syntax.Expression as MemberAccessExpressionSyntax)?.Expression ??
            syntax.ArgumentList.Arguments.FirstOrDefault()?.Expression) as ObjectCreationExpressionSyntax;

    static RegexOptions GetConstantValue(SemanticModel model, IEnumerable<ArgumentSyntax>? rest) =>
        rest?.FirstOrDefault() is { } options &&
        model.GetConstantValue(options.Expression) is { HasValue: true, Value: int constant }
            ? (RegexOptions)constant
            : RegexOptions.None;

    static IMethodSymbol? TryGetRegexDeconstructMethod(
        InvocationExpressionSyntax syntax,
        SemanticModel model,
        CancellationToken token
    ) =>
        model.GetSymbolInfo(syntax.Expression, token).Symbol is IMethodSymbol
        {
            Name: RegexGenerator.MethodName,
            ReturnType.Name: nameof(Boolean),
            ContainingNamespace.Name: nameof(Emik),
            ContainingType.Name: RegexGenerator.TypeName,
        } method
            ? method
            : null;

    static BaseObjectCreationExpressionSyntax? Initializer(SyntaxNode? symbol) =>
        symbol switch
        {
            VariableDeclaratorSyntax x => x.Initializer?.Value,
            BaseMethodDeclarationSyntax x => x.ExpressionBody?.Expression,
            PropertyDeclarationSyntax x => x.Initializer?.Value ?? x.ExpressionBody?.Expression,
            _ => null,
        } as BaseObjectCreationExpressionSyntax;

    static IdentifierNameSyntax? Identifier(MemberAccessExpressionSyntax? arg) =>
        arg?.Expression switch
        {
            IdentifierNameSyntax x => x,
            InvocationExpressionSyntax x => x.Expression as IdentifierNameSyntax,
            _ => null,
        };

    static bool IsFromRegexGenerator(ImmutableArray<IParameterSymbol>.Enumerator enumerator)
    {
        if (!enumerator.MoveNext())
            return false;

        if (enumerator.Current is { RefKind: RefKind.None, Type.Name: nameof(Regex) } && !enumerator.MoveNext())
            return false;

        if (enumerator.Current is not { RefKind: RefKind.None, Type.Name: nameof(String) } || !enumerator.MoveNext())
            return false;

        do
            if (!IsGroup(enumerator.Current))
                return false;
        while (enumerator.MoveNext());

        return true;
    }

    static bool IsGeneratedRegex(AttributeData data) => data.AttributeClass?.Name is nameof(GeneratedRegexAttribute);

    static bool IsGroup(IParameterSymbol parameter) => parameter is { RefKind: RefKind.Out, Type.Name: nameof(Group) };
}
