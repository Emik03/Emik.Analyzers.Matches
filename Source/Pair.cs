// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

/// <summary>Represents a regex that has been used.</summary>
sealed class Pair
{
    readonly Regex _regex;

    Pair(Regex regex) => (_regex, LastUsed) = (regex, DateTime.Now);

    /// <summary>Gets the last time the regex has been obtained.</summary>
    public DateTime LastUsed { get; private set; }

    /// <summary>Gets the regex, updating the last time it has been used.</summary>
    /// <returns>The regex stored within this instance.</returns>
    public Regex GetAndUpdate()
    {
        LastUsed = DateTime.Now;
        return _regex;
    }

    public static implicit operator Pair(Regex regex) => new(regex);
}
