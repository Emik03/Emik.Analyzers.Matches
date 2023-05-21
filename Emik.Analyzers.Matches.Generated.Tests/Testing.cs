// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches.Generated.Tests;
#pragma warning disable 0219, CA1819, IDE0059, MA0110

// ReSharper disable NotAccessedPositionalProperty.Global
partial record B([Match(@"^\d*$")] string? Unused = default)
{
    static readonly Regex s_fieldRegex = new("a(b)");

    static Regex PropertyRegexBody => new("a(b)");

    static Regex PropertyRegexInitializer { get; } = new("a(b)");

    public string this[[Match(@"^\d*$")] string a] => a;

    [GeneratedRegex("foobar(a)")]
    private static partial Regex PartialMethodRegex();

    static Regex MethodBodyRegex() => new("a(b)");

    public static void DoesItWork()
    {
        // ReSharper disable once StringLiteralTypo
        const string
            No = "this should fail",
            Yes = "017893567891",
            NotGood = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaatt";

        Regex regex = new("foobar(a)");

        // This should pass.
        _ = Static(Yes);
        _ = new B().Instance(Yes);
        _ = new B()[Yes];
        B unused1 = new(Yes), unused2 = new(Yes);
        B unused3 = Yes, unused4 = Yes;
        Discard(Yes);
        Static(Yes, Yes, Yes, Yes);

        _ = new Regex("(a)(b)").Match("");
        PartialMethodRegex().Match("", out _, out _);
        new Regex("foobar(a)").Match("", out _, out _);
        s_fieldRegex.Match("", out _, out _);
        PropertyRegexBody.Match("", out _, out _);
        PropertyRegexInitializer.Match("", out _, out _);
        regex.Match("", out _, out _);
        MethodBodyRegex().Match("", out _, out _);

        // These should all error.
        _ = Static(No);
        _ = new B().Instance(No);
        _ = new B()[No];
        B unused5 = new(No), unused6 = new(No);
        B unused7 = No, unused8 = No;
        Discard(No);
        Static(No, No, No, No);

        PartialMethodRegex().Match("", out _);
        new Regex("foobar(a)").Match("", out _);
        s_fieldRegex.Match("", out _);
        PropertyRegexBody.Match("", out _);
        PropertyRegexInitializer.Match("", out _);
        regex.Match("", out _);
        MethodBodyRegex().Match("", out _);

        // These should all give a warning.
        _ = Static(Yes[..]);
        _ = new B().Instance(Yes[..]);
        _ = new B()[Yes[..]];
        B unused9 = new(Yes[..]), unused10 = new(Yes[..]);
        B unused11 = Yes[..], unused12 = Yes[..];
        Discard(Yes[..]);



        // This should give a hint.
        EvilRegex(NotGood);
    }

    public static void Discard(B _) { }

    public static string[] Static([Match(@"^\d*$")] params string[] a) => a;

    public static string Static([Match(@"^\d*$")] string a) => a;

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public string Instance([Match(@"^\d*$")] string a) => a;

    // ReSharper disable once MemberCanBeMadeStatic.Global
    public static string EvilRegex([Match(@"^([^t]+)+t$")] string a) => a;

    public static implicit operator B([Match(@"^\d*$")] string _) => new();
}

public record Ab([Match("True")] params bool[] Value2);

public record Aa() : Ab(true, false);
