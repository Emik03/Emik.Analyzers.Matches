// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches.Generated.Tests;
#pragma warning disable 0219, CA1819, IDE0059, MA0110, RCS1085

// ReSharper disable NotAccessedPositionalProperty.Global
partial record B([Match(@"^\d*$")] string? Unused = default)
{
    // ReSharper disable once StringLiteralTypo
    const string
        No = "this should fail",
        Yes = "017893567891",
        NotGood = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaatt";

    static readonly Regex s_runtimeFieldRegex = Random.Shared.Next(2) is 0 ? new("foo") : new("bar");

    static readonly Regex s_fieldRegex = new("a(b)");

    static Regex PropertyRegexBody => new("a(b)");

    static Regex PropertyRegexInitializer { get; } = new("a(b)");

    // ReSharper disable once ConvertToAutoPropertyWhenPossible
    static Regex RuntimePropertyRegexBody => s_runtimeFieldRegex;

    // ReSharper disable once ReplaceAutoPropertyWithComputedProperty
    static Regex RuntimePropertyRegexInitializer { get; } = s_runtimeFieldRegex;

    public string this[[Match(@"^\d*$")] string a] => a;

    [GeneratedRegex("foobar(a)")]
    private static partial Regex PartialMethodRegex();

    static Regex MethodBodyRegex() => new("a(b)");

    static Regex RuntimeMethodBodyRegex() => s_runtimeFieldRegex;

    public static void DoesItWork()
    {
        Regex regex = new("foobar(a)");

        _ = Static(Yes);
        _ = new B().Instance(Yes);
        _ = new B()[Yes];
        B unused1 = new(Yes), unused2 = new(Yes);
        B unused3 = Yes, unused4 = Yes;
        Discard(Yes);
        Static(Yes, Yes, Yes, Yes);
        Interpolation($"{Yes} {Yes:Yes}");

        PartialMethodRegex().IsMatch("", out _, out _);
        _ = new Regex("(a)(b)").IsMatch("");
        new Regex("foobar(a)").IsMatch("", out _, out _);
        s_fieldRegex.IsMatch("", out _, out _);
        PropertyRegexBody.IsMatch("", out _, out _);
        PropertyRegexInitializer.IsMatch("", out _, out _);
        regex.IsMatch("", out _, out _);
        MethodBodyRegex().IsMatch("", out _, out _);
    }

    public void Error()
    {
        Regex regex = new("foobar(a)");

        _ = Static(No);
        _ = new B().Instance(No);
        _ = new B()[No];
        B unused5 = new(No), unused6 = new(No);
        B unused7 = No, unused8 = No;
        Discard(No);
        Static(No, No, No, No);
        Interpolation($"{No} {No:No}");

        new Regex("foobar(a)").IsMatch("", out _);
        s_fieldRegex.IsMatch("", out _);
        PropertyRegexBody.IsMatch("", out _);
        PropertyRegexInitializer.IsMatch("", out _);
        regex.IsMatch("", out _);
        MethodBodyRegex().IsMatch("", out _);
    }

    public void Warning()
    {
        var runtimeRegex = s_runtimeFieldRegex;

        _ = Static(Yes[..]);
        _ = new B().Instance(Yes[..]);
        _ = new B()[Yes[..]];
        B unused9 = new(Yes[..]), unused10 = new(Yes[..]);
        B unused11 = Yes[..], unused12 = Yes[..];
        Discard(Yes[..]);
        Interpolation($"{Yes} {Yes:Yes}");

        s_runtimeFieldRegex.IsMatch("", out _, out _, out _, out _);
        RuntimePropertyRegexBody.IsMatch("", out _, out _);
        RuntimePropertyRegexInitializer.IsMatch("", out _, out _);
        runtimeRegex.IsMatch("", out _, out _);
        RuntimeMethodBodyRegex().IsMatch("", out _, out _);
    }

    public void Hint() => EvilRegex(NotGood);

    public static void Discard(B _) { }

    public static string[] Static([Match(@"^\d*$")] params string[] a) => a;

    public static string Static([Match(@"^\d*$")] string a) => a;

    public static string Interpolation(InterpolatedStringHandler a) => throw Unreachable;

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string Instance([Match(@"^\d*$")] string a) => a;

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public static string EvilRegex([Match("^([^t]+)+t$")] string a) => a;

    public static implicit operator B([Match(@"^\d*$")] string _) => new();

    /// <summary>
    /// Provides a handler used by the language compiler to process interpolated strings into <see cref="string"/> instances.
    /// </summary>
    /// <param name="literalLength">
    /// The number of constant characters outside of interpolation expressions in the interpolated string.
    /// </param>
    /// <param name="formattedCount">The number of interpolation expressions in the interpolated string.</param>
    /// <param name="provider">An object that supplies culture-specific formatting information.</param>
    [InterpolatedStringHandler]
    public readonly ref struct InterpolatedStringHandler(
        int literalLength,
        int formattedCount,
        IFormatProvider? provider = default
    )
    {
        readonly StringBuilder _builder = new(literalLength);

        /// <summary>Writes the specified string to the handler.</summary>
        /// <param name="value">The string to write.</param>
        public void AppendLiteral([Match(@"^\d*$")] string? value) => _builder.Append(value);

        /// <summary>Writes the specified value to the handler.</summary>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The format string.</param>
        /// <typeparam name="T">The type of the value to write.</typeparam>
        public void AppendFormatted(B value, B? format = default) => throw Unreachable;
    }
}

public record Ab([Match("True")] params bool[] Value2);

public record Aa() : Ab(true, false);
