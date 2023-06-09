# Emik.Analyzers.Matches

[![NuGet package](https://img.shields.io/nuget/v/Emik.Analyzers.Matches.svg?logo=NuGet)](https://www.nuget.org/packages/Emik.Analyzers.Matches)
[![License](https://img.shields.io/github/license/Emik03/Emik.Analyzers.Matches.svg?style=flat)](https://github.com/Emik03/Emik.Analyzers.Matches/blob/main/LICENSE)

Analyzer for compile-time parameter validation with the power of regex.

This project has a dependency to [Emik.Morsels](https://github.com/Emik03/Emik.Morsels), if you are building this
project, refer to its [README](https://github.com/Emik03/Emik.Morsels/blob/main/README.md) first.

---

- [Example](#example)
- [Lints](#lints)
- [Contribute](#contribute)
- [License](#license)

---

## Example

```csharp
[StringSyntax(StringSyntaxAttribute.Regex)]
const string ValidatorQuery = @"^\d{2}$";

[GeneratedRegex(ValidatorQuery)]
partial Regex Validator();

byte? Test([Emik.Match(ValidatorQuery)] string x)
{
    if (!Validator().IsMatch(out var capture)) // OK
        return null;

    if (!Validator().IsMatch(out _, out var captureThatDoesNotExist)) // Error
        return null;

    if (!new Regex(x).IsMatch(out _)) // Warning: Not a constant
        return null;

    return byte.Parse(capture);
}

string TestTyped(X x) => x.Value;

record X(string Value)
{
    public static implicit operator X([Match(@"^\w{3}$")] string value) => new(value);
}

Test("12"); // OK
Test("34"); // OK
TestTyped("foo"); // OK
TestTyped("bar"); // OK

Test("1"); // Error
Test("foobar"); // Error
TestTyped("12"); // Error
TestTyped("food"); // Error

Test(bool.FalseString); // Warning: Not a constant
TestTyped(bool.TrueString); // Warning: Not a constant
```

## Lints

| Id                                                                                             | Title                                                      |
|------------------------------------------------------------------------------------------------|------------------------------------------------------------|
| [EAM001](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM001.md) | Argument fails regex test                                  |
| [EAM002](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM002.md) | Non-constant argument may fail regex test                  |
| [EAM003](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM003.md) | Argument regex test took too long                          |
| [EAM004](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM004.md) | Capture group doesn't exist                                |
| [EAM005](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM005.md) | Missing out declaration for capture group                  |
| [EAM006](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM006.md) | Non-constant Regex may have wrong number of capture groups |

## Contribute

Issues and pull requests are welcome to help this repository be the best it can be.

## License

This repository falls under the [MPL-2 license](https://www.mozilla.org/en-US/MPL/2.0/).
