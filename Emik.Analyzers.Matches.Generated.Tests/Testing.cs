// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches.Generated.Tests;

static class Extensions
{
    public static bool Test(Type t, bool b, [Match("^$")] string a) => b;
}

partial record B([Match(@"^\d*$")] string? Unused = default)
{
    static readonly Regex s_fieldRegex = new("a(b)");

    static Regex PropertyRegexBody => new("a(b)");

    static Regex PropertyRegexInitializer { get; } = new("a(b)");

    public string this[[Match(@"^\d*$")] string a] => a;

    [GeneratedRegex("foobar(a)")]
    private static partial Regex PartialMethodRegex();

    static Regex MethodBodyRegex() => new("a(b)");

    // Assembly 1
    public static Regex Rgx() => new("foobar(a)(b)");

    // Assembly 2
    public static void DoesItWork()
    {
        const string Yes = "017893567891";

        PartialMethodRegex().Match("", out var a, out var b);
        new Regex("foobar(a)").Match("", out var c, out var d);
        s_fieldRegex.Match("", out var e, out var ef);
        PropertyRegexBody.Match("", out var g, out var h);
        PropertyRegexInitializer.Match("", out var i, out var j);
        MethodBodyRegex().Match("", out var k, out var l);

        Regex regex = new Regex("foobar(a)(b)");

        Rgx().Match("", out var x, out var y, out var z);

        new Regex("").Match("");

        // This should pass.
        _ = Static(Yes);
        _ = new B().Instance(Yes);
        _ = new B()[Yes];
        B unused1 = new(Yes), unused2 = new(Yes);
        B unused3 = Yes, unused4 = Yes;
        Discard(Yes);

        // These should all error.
        _ = Static("this should fail");
        _ = new B().Instance("this should fail");
        _ = new B()["this should fail"];
        B unused5 = new("this should fail"), unused6 = new("this should fail");
        B unused7 = "this should fail", unused8 = "this should fail";
        Discard("this shosuld fail");

        // These should all give a warning.
        _ = Static(Yes[..]);
        _ = new B().Instance(Yes[..]);
        _ = new B()[Yes[..]];
        B unused9 = new(Yes[..]), unused10 = Yes[..];
        B unused11 = Yes[..], unused12 = Yes[..];
        Discard(Yes[..]);

        Static("", "", "", "");

        // This should give a hint.
        EvilRegex("bingbingbang ana boonk ana bonk ana ting witta tingtang");
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
