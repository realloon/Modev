namespace Modev;

public sealed class ModevSettings : ModSettings {
    private List<string> _excludedRules = [];
    public bool SkipWorkshopConfirmDelay = true;
    public bool IgnoreDotPrefixedPaths;

    public override void ExposeData() {
        Scribe_Collections.Look(ref _excludedRules, "excludedRules", LookMode.Value);
        Scribe_Values.Look(ref SkipWorkshopConfirmDelay, "skipWorkshopConfirmDelay", true);
        Scribe_Values.Look(ref IgnoreDotPrefixedPaths, "ignoreDotPrefixedPaths");
        _excludedRules ??= [];
        CanonicalizeRulesInPlace(_excludedRules);
    }

    public List<string> GetExcludedFolders() {
        CanonicalizeRulesInPlace(_excludedRules);
        return [.._excludedRules.Where(IsFolderRule).Select(ToFolderPath)];
    }

    public List<string> GetExcludedFiles() {
        CanonicalizeRulesInPlace(_excludedRules);
        return [.._excludedRules.Where(rule => !IsFolderRule(rule))];
    }

    public List<string> GetExcludedRules() {
        CanonicalizeRulesInPlace(_excludedRules);
        return [.._excludedRules];
    }

    public bool TryAddExcludedRule(string rawInput) {
        var normalizedRule = NormalizeRule(rawInput);
        if (normalizedRule is not { Length: > 0 }) {
            return false;
        }

        if (_excludedRules.Any(existing => existing.Equals(normalizedRule, StringComparison.OrdinalIgnoreCase))) {
            return false;
        }

        _excludedRules.Add(normalizedRule);
        CanonicalizeRulesInPlace(_excludedRules);
        return true;
    }

    public void RemoveExcludedRuleAt(int sortedIndex) {
        CanonicalizeRulesInPlace(_excludedRules);
        if (sortedIndex < 0 || sortedIndex >= _excludedRules.Count) {
            return;
        }

        _excludedRules.RemoveAt(sortedIndex);
    }

    private static bool IsFolderRule(string rule) {
        return rule.EndsWith("/", StringComparison.Ordinal);
    }

    private static string ToFolderPath(string folderRule) {
        return folderRule[..^1];
    }

    private static void CanonicalizeRulesInPlace(List<string> rules) {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var i = rules.Count - 1; i >= 0; i--) {
            var normalized = NormalizeRule(rules[i]);
            if (normalized is not { Length: > 0 } || !seen.Add(normalized)) {
                rules.RemoveAt(i);
                continue;
            }

            rules[i] = normalized;
        }

        rules.Sort(CompareRules);
    }

    private static int CompareRules(string? a, string? b) {
        var aFolder = a != null && IsFolderRule(a);
        var bFolder = b != null && IsFolderRule(b);
        if (aFolder != bFolder) {
            return aFolder ? -1 : 1;
        }

        return string.Compare(a, b, StringComparison.OrdinalIgnoreCase);
    }

    private static string? NormalizeRule(string value) {
        var isFolderRule = value.TrimEnd().EndsWith("/", StringComparison.Ordinal);
        var path = value.Trim().Replace('\\', '/');
        while (path.StartsWith("/", StringComparison.Ordinal)) {
            path = path[1..];
        }

        while (path.EndsWith("/", StringComparison.Ordinal)) {
            path = path[..^1];
        }

        if (path.Length == 0) return null;

        return isFolderRule ? path + "/" : path;
    }
}