// SPDX-License-Identifier: MPL-2.0
namespace Emik.Analyzers.Matches;

using CachePair = KeyValuePair<(string, RegexOptions), Pair>;

/// <summary>Handles regex caching.</summary>
public static class RegexCache
{
    // 256 is a common allocation block size. Prevent dictionary from becoming any larger.
    const int MaxCacheSize = 256;

    static readonly TimeSpan s_maxMatchingTime = TimeSpan.FromMilliseconds(200),
        s_maxCacheTime = TimeSpan.FromMinutes(5);

    static readonly ConcurrentDictionary<(string, RegexOptions), Pair> s_cache = new();

    /// <summary>Gets the regex corresponding to the pattern and options, cached if possible.</summary>
    /// <param name="pattern">The pattern of the regular expression.</param>
    /// <param name="options">The options of the regular expression.</param>
    /// <returns>The <see cref="Regex"/> instance, if compilable.</returns>
    public static Regex? Get(string pattern, RegexOptions options)
    {
        var key = (pattern, options);

        if (s_cache.TryGetValue(key, out var cached))
            return cached.GetAndUpdate();

        if (TryCreate(pattern, options) is not { } regex)
            return null;

        if (s_cache.Count >= MaxCacheSize)
            ClearOldRegexes();

        s_cache[key] = regex;
        return regex;
    }

    static void ClearOldRegexes() => s_cache.Where(IsOld).Append(s_cache.Aggregate(Older)).ToList().ForEach(TryRemove);

    static void TryRemove(CachePair x) => s_cache.TryRemove(x.Key, out _);

    static Regex? TryCreate(string pattern, RegexOptions options)
    {
        try
        {
            return new(pattern, options | RegexOptions.Compiled, s_maxMatchingTime);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    static bool IsOld(CachePair x) => x.Value.LastUsed - DateTime.Now > s_maxCacheTime;

    static CachePair Older(CachePair prev, CachePair next) => prev.Value.LastUsed < next.Value.LastUsed ? prev : next;
}
