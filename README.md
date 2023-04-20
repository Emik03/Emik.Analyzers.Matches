# Emik.Analyzers.Matches

[![NuGet package](https://img.shields.io/nuget/v/Emik.Analyzers.Matches.svg?logo=NuGet)](https://www.nuget.org/packages/Emik.Analyzers.Matches)
[![License](https://img.shields.io/github/license/Emik03/Emik.Analyzers.Matches.svg?style=flat)](https://github.com/Emik03/Emik.Analyzers.Matches/blob/main/LICENSE)

Analyzer for compile-time parameter validation with the power of regex.

This project has a dependency to [Emik.Morsels](https://github.com/Emik03/Emik.Morsels), if you are building this project, refer to its [README](https://github.com/Emik03/Emik.Morsels/blob/main/README.md) first.

Example usage:

```csharp
string Test([Match(@"^\d{2}$")] string x) => x;

Test("12"); // No warning.
Test("34"); // No warning.

Test("1"); // Warning.
Test("foobar"); // Warning.
```

---

- [Contribute](#contribute)
- [License](#license)

---

## Contribute

Issues and pull requests are welcome to help this repository be the best it can be.

## License

This repository falls under the [MPL-2 license](https://www.mozilla.org/en-US/MPL/2.0/).
