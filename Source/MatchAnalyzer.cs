// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

using Couple = (AttributeData? Data, SmallList<IMethodSymbol> AppendFormatted, IMethodSymbol? AppendLiteral);

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

    static void Go(in SyntaxNodeAnalysisContext context, BaseArgumentListSyntax? argumentList)
    {
        var (compilation, token, node, model) =
            (context.Compilation, context.CancellationToken, context.Node, context.SemanticModel);

        IEnumerable<Diagnostic?> Diagnostics(IEnumerable<IParameterSymbol> parameters) =>
            parameters
               .Select(GetAttributesIncludingInformationFromInterpolatedStringHandlerAttribute)
               .Select(ExpressionFromArgumentList)
               .SelectMany(x => LocateInterpolationMismatches(x).Prepend(LocateMismatches(x.Couple.Data, x.Syntax)));

        IEnumerable<Diagnostic?> ManyDiagnostics(InterpolatedStringContentSyntax x) =>
            Diagnostics((model.GetSymbolInfo(x, token).Symbol as IMethodSymbol)?.Parameters);

        (Couple Couple, ExpressionSyntax? Syntax) ExpressionFromArgumentList(Couple couple, int index) =>
            (couple, argumentList?.Arguments.Nth(index)?.Expression);

        IEnumerable<Diagnostic?> LocateInterpolationMismatches((Couple Couple, ExpressionSyntax? Syntax) tuple) =>
            tuple is ((_, var formatted, { } literal), InterpolatedStringExpressionSyntax { Contents: [_, ..] xs }) &&
            GetAttributes(literal.Parameters.FirstOrDefault()) is var data
                ? xs.SelectMany(x => x is InterpolationSyntax i ? Enumerable(i, formatted) : LocateMismatches(data, x).Yield())
                : [];

        Diagnostic? LocateMismatches(AttributeData? data, SyntaxNode? node) =>
            node is not null &&
            data?.ConstructorArguments is [{ Value: string pattern }, { Value: int options }] &&
            model.GetConstantValue(node, token) is var constant &&
            Match(constant, pattern, options) is var match &&
            Descriptors.From(match) is { } descriptor
                ? Diagnostic.Create(descriptor, node.GetLocation(), pattern)
                : null;

        (node switch
            {
                ExpressionSyntax expression => Diagnostics(Parameters(expression, model, token)),
                PrimaryConstructorBaseTypeSyntax primary => Diagnostics(Parameters(primary, model, token)),
                VariableDeclaratorSyntax
                    {
                        Initializer.Value: var value, Parent: VariableDeclarationSyntax { Type: var type },
                    } when model.GetTypeInfo(value, token).Type?.SpecialType is SpecialType.System_String &&
                    model.GetTypeInfo(type, token).Type is { } symbol
                    => GetAttributes(symbol).Select(x => LocateMismatches(x, value)),
                _ => [],
            })
           .Filter()
           .Lazily(context.ReportDiagnostic)
           .Enumerate();

        IEnumerable<Diagnostic?> Enumerable(InterpolationSyntax interpolation, SmallList<IMethodSymbol> formatted)
        {
            var expression = model.GetTypeInfo(interpolation.Expression, token).Type;
            var format = TypeIn(interpolation.FormatClause);
            var alignment = TypeIn(interpolation.AlignmentClause);

            InvocationExpressionSyntax.DeserializeFrom(new MemoryStream(), default);

            Predicate<IMethodSymbol>? Predicate(Strictness s) => // ReSharper disable AccessToModifiedClosure
                s is Strictness.Exact
                    ? x => TypeSymbolComparer.Equal(x.Parameters.FirstOrDefault()?.Type, expression) &&
                        TypeSymbolComparer.Equal(Alignment(x)?.Type, alignment) &&
                        TypeSymbolComparer.Equal(Format(x)?.Type, format)
                    : x => expression.IsAssignableTo(x.Parameters.FirstOrDefault()?.Type, compilation) &&
                        alignment.IsAssignableTo(Alignment(x)?.Type, compilation) &&
                        format.IsAssignableTo(Format(x)?.Type, compilation) &&
                        s switch
                        {
                            Strictness.Exact => throw Unreachable,
                            Strictness.ImplicitlyAndConstrained =>
                                expression is ITypeParameterSymbol { ConstraintTypes.IsEmpty: false } ||
                                alignment is ITypeParameterSymbol { ConstraintTypes.IsEmpty: false } ||
                                format is ITypeParameterSymbol { ConstraintTypes.IsEmpty: false },
                            Strictness.ImplicitlyAndUnconstrained =>
                                expression is ITypeParameterSymbol ||
                                alignment is ITypeParameterSymbol ||
                                format is ITypeParameterSymbol,
                            Strictness.Implicitly => true,
                            _ => throw new InvalidOperationException($"{s}"),
                        };

            // ReSharper restore AccessToModifiedClosure

            var method = formatted.FirstOrDefault(Predicate(Strictness.Exact)) ??
                formatted.FirstOrDefault(Predicate(Strictness.ImplicitlyAndConstrained)) ??
                formatted.FirstOrDefault(Predicate(Strictness.ImplicitlyAndUnconstrained)) ??
                formatted.FirstOrDefault(Predicate(Strictness.Implicitly));

            if (method is null)
                yield break;

            yield return LocateMismatches(
                GetAttributes(method.Parameters.FirstOrDefault()),
                interpolation.Expression
            );

            yield return LocateMismatches(GetAttributes(Alignment(method)), interpolation.AlignmentClause);

            yield return LocateMismatches(GetAttributes(Format(method)), interpolation.FormatClause);
        }

        ITypeSymbol? TypeIn(SyntaxNode? node) => node is null ? null : model.GetTypeInfo(node, token).Type;
    }

    static IParameterSymbol? Alignment(IMethodSymbol x) => x.Parameters.FirstOrDefault(x => x.Name is "alignment");

    static IParameterSymbol? Format(IMethodSymbol x) => x.Parameters.FirstOrDefault(x => x.Name is "format");

    static Couple GetAttributesIncludingInformationFromInterpolatedStringHandlerAttribute(IParameterSymbol parameter) =>
        GetAttributes(parameter) is var first &&
        parameter.Type
           .GetMembers()
           .OfType<IMethodSymbol>()
           .ToSmallList() is var list &&
        parameter.Type
           .GetAttributes()
           .All(x => x.AttributeClass?.Name is not nameof(InterpolatedStringHandlerAttribute))
            ? (first, default, default)
            : (first, FindAppendFormattedCandidates(list), FindAppendLiteral(list));

    static AttributeData? GetAttributes(IParameterSymbol? parameter) =>
        parameter
          ?.GetAttributes()
           .AddRange(GetAttributes(parameter.Type))
           .FirstOrDefault(x => x.AttributeClass?.Name is MatchGenerator.TypeName);

    static IMethodSymbol? FindAppendLiteral(SmallList<IMethodSymbol> list)
    {
        static bool IsAlone(IMethodSymbol x) => x.Parameters.Length is 1;

        static bool IsFirstImplicitlyString(IMethodSymbol x) =>
            x.Parameters is [{ Type: var type }, ..] && type.TryFindSingleMethod(IsConversion, out _);

        static bool IsFirstString(IMethodSymbol x) =>
            x.Parameters.FirstOrDefault()?.Type.SpecialType is SpecialType.System_String;

        static bool IsRestOptional(IMethodSymbol x)
        {
            for (var i = 1; i < x.Parameters.Length; i++)
                if (x.Parameters[i] is { IsOptional: false, IsParams: false })
                    return false;

            return true;
        }

        var literals = list.Where(x => x.Name is nameof(Couple.AppendLiteral)).ToSmallList();

        return literals.FirstOrDefault(x => IsFirstString(x) && IsAlone(x)) ??
            literals.FirstOrDefault(x => IsFirstString(x) && IsRestOptional(x)) ??
            literals.FirstOrDefault(x => IsFirstImplicitlyString(x) && IsAlone(x)) ??
            literals.FirstOrDefault(x => IsFirstImplicitlyString(x) && IsRestOptional(x));
    }

    static SmallList<IMethodSymbol> FindAppendFormattedCandidates(SmallList<IMethodSymbol> list)
    {
        static bool IsValid(IMethodSymbol x)
        {
            if (x.Name is not nameof(Couple.AppendFormatted))
                return false;

            for (var i = 1; i < x.Parameters.Length; i++)
                if (x.Parameters[i] is { IsOptional: false, IsParams: false, Name: not "alignment" and not "format" })
                    return false;

            return true;
        }

        return list.Where(IsValid).ToSmallList();
    }

    static IEnumerable<AttributeData> GetAttributes(ITypeSymbol type) =>
        type.TryFindSingleMethod(IsConversion, out var conversion)
            ? conversion.Parameters.SelectMany(x => x.GetAttributes())
            : [];

    static bool IsConversion(IMethodSymbol x) =>
        x is { MethodKind: MethodKind.Conversion, Parameters: [{ Type.SpecialType: SpecialType.System_String }] };

    static bool IsIntConversion(IMethodSymbol x) =>
        x is { MethodKind: MethodKind.Conversion, Parameters: [{ Type.SpecialType: SpecialType.System_String }] };

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
