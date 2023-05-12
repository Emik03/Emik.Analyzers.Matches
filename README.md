# Emik.Analyzers.Matches

[![NuGet package](https://img.shields.io/nuget/v/Emik.Analyzers.Matches.svg?logo=NuGet)](https://www.nuget.org/packages/Emik.Analyzers.Matches)
[![License](https://img.shields.io/github/license/Emik03/Emik.Analyzers.Matches.svg?style=flat)](https://github.com/Emik03/Emik.Analyzers.Matches/blob/main/LICENSE)

Analyzer for compile-time parameter validation with the power of regex.

This project has a dependency to [Emik.Morsels](https://github.com/Emik03/Emik.Morsels), if you are building this project, refer to its [README](https://github.com/Emik03/Emik.Morsels/blob/main/README.md) first.

---

- [Example](#example)
- [Lints](#lints)
- [Contribute](#contribute)
- [License](#license)

---

## Example

```csharp
string Test([Emik.Match(@"^\d{2}$")] string x) => x;

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

Test(System.Environment.NewLine); // Warning: Not a constant
TestTyped(System.Environment.NewLine); // Warning: Not a constant
```

## Lints

| Id                                                                                             | Title                                     |
|------------------------------------------------------------------------------------------------|-------------------------------------------|
| [EAM001](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM001.md) | Argument fails regex test                 |
| [EAM002](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM002.md) | Non-constant argument may fail regex test |
| [EAM003](https://github.com/Emik03/Emik.Analyzers.Matches/tree/master/Documentation/EAM003.md) | Argument regex test took too long         |

## Contribute

Issues and pull requests are welcome to help this repository be the best it can be.

## License

This repository falls under the [MPL-2 license](https://www.mozilla.org/en-US/MPL/2.0/).
