namespace Modev.Utility;

public static class UploadContentFilter {
    public static DirectoryInfo BuildFilteredCopy(DirectoryInfo source, IReadOnlyCollection<string> excludedRules,
        bool ignoreDotPrefixedPaths) {
        var root = Path.Combine(GenFilePaths.TempFolderPath, "Vortex_Modev");
        Directory.CreateDirectory(root);

        var targetPath = Path.Combine(root, source.Name + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
        CopyDirectory(source.FullName, targetPath, string.Empty, excludedRules, ignoreDotPrefixedPaths);
        return new DirectoryInfo(targetPath);
    }

    public static List<string> ListIncludedTopLevelPaths(DirectoryInfo source,
        IReadOnlyCollection<string> excludedRules, bool ignoreDotPrefixedPaths) {
        return [
            .. source.EnumerateFileSystemInfos()
                .Where(entry => !ShouldSkip(entry.Name, entry.Name, entry is DirectoryInfo,
                    excludedRules, ignoreDotPrefixedPaths))
                .OrderBy(entry => entry is DirectoryInfo ? 0 : 1)
                .ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .Select(entry => entry is DirectoryInfo ? entry.Name + "/" : entry.Name)
        ];
    }

    public static void CopyDirectory(string sourcePath, string targetPath) {
        CopyDirectory(sourcePath, targetPath, string.Empty, [], false);
    }

    private static void CopyDirectory(string sourcePath, string targetPath, string relativePath,
        IReadOnlyCollection<string> excludedRules, bool ignoreDotPrefixedPaths) {
        Directory.CreateDirectory(targetPath);

        foreach (var filePath in Directory.EnumerateFiles(sourcePath)) {
            var fileName = Path.GetFileName(filePath);
            var relativeFilePath = relativePath.Length == 0
                ? fileName
                : relativePath + "/" + fileName;
            if (ShouldSkip(fileName, relativeFilePath, false, excludedRules, ignoreDotPrefixedPaths)) {
                continue;
            }

            var targetFile = Path.Combine(targetPath, fileName);
            File.Copy(filePath, targetFile, true);
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(sourcePath)) {
            var folderName = Path.GetFileName(directoryPath);
            var childRelativePath = relativePath.Length == 0
                ? folderName
                : relativePath + "/" + folderName;

            if (ShouldSkip(folderName, childRelativePath, true, excludedRules, ignoreDotPrefixedPaths)) continue;

            var targetChild = Path.Combine(targetPath, folderName);
            CopyDirectory(directoryPath, targetChild, childRelativePath, excludedRules, ignoreDotPrefixedPaths);
        }
    }

    private static bool ShouldSkip(string name, string relativePath, bool isFolder,
        IReadOnlyCollection<string> excludedRules, bool ignoreDotPrefixedPaths) {
        return (ignoreDotPrefixedPaths && name.StartsWith(".", StringComparison.Ordinal)) ||
               IsExcluded(relativePath, isFolder, excludedRules);
    }

    private static bool IsExcluded(string relativePath, bool isFolder, IReadOnlyCollection<string> excludedRules) {
        return excludedRules.Any(rule => IsRuleMatch(relativePath, isFolder, rule));
    }

    private static bool IsRuleMatch(string candidate, bool isFolder, string rule) {
        if (!rule.EndsWith("/", StringComparison.Ordinal)) {
            return !isFolder && candidate.Equals(rule, StringComparison.OrdinalIgnoreCase);
        }

        var folder = rule[..^1];
        return (isFolder && candidate.Equals(folder, StringComparison.OrdinalIgnoreCase)) ||
               candidate.StartsWith(rule, StringComparison.OrdinalIgnoreCase);
    }
}