// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches.Generated.Tests;

static class Extensions
{
    public static bool Test(Type t, bool b, [Match("^$")] string a) => b;
}

record B([Match(@"^\d*$")] string? Unused = default)
{
    public string this[[Match(@"^\d*$")] string a] => a;

    public static void DoesItWork()
    {
        const string Yes = "017893567891";

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
