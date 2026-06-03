namespace Modev.Utility;

public static class UploadContentFilter {
    public static DirectoryInfo BuildFilteredCopy(DirectoryInfo source, IReadOnlyCollection<string> excludedRules,
        bool ignoreDotPrefixedPaths) {
        var root = Path.Combine(GenFilePaths.TempFolderPath, "Vortex_Modev");
        Directory.CreateDirectory(root);
        CleanupAllPreviousDirectories(root);

        var targetPath = Path.Combine(root, source.Name + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
        CopyDirectory(source.FullName, targetPath, string.Empty, excludedRules, ignoreDotPrefixedPaths);
        return new DirectoryInfo(targetPath);
    }

    public static List<string> ListIncludedTopLevelPaths(DirectoryInfo source,
        IReadOnlyCollection<string> excludedRules, bool ignoreDotPrefixedPaths) {
        var paths = new List<string>();

        foreach (var directoryPath in Directory.GetDirectories(source.FullName)
                     .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)) {
            var folderName = Path.GetFileName(directoryPath);
            if (ignoreDotPrefixedPaths && folderName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            if (IsExcluded(folderName, true, excludedRules)) {
                continue;
            }

            paths.Add(folderName + "/");
        }

        foreach (var filePath in Directory.GetFiles(source.FullName)
                     .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)) {
            var fileName = Path.GetFileName(filePath);
            if (ignoreDotPrefixedPaths && fileName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            if (IsExcluded(fileName, false, excludedRules)) {
                continue;
            }

            paths.Add(fileName);
        }

        return paths;
    }

    private static void CleanupAllPreviousDirectories(string rootPath) {
        foreach (var dir in new DirectoryInfo(rootPath).GetDirectories()) {
            dir.Delete(true);
        }
    }

    private static void CopyDirectory(string sourcePath, string targetPath, string relativePath,
        IReadOnlyCollection<string> excludedRules, bool ignoreDotPrefixedPaths) {
        Directory.CreateDirectory(targetPath);

        foreach (var filePath in Directory.GetFiles(sourcePath)) {
            var fileName = Path.GetFileName(filePath);
            if (ignoreDotPrefixedPaths && fileName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            var relativeFilePath = string.IsNullOrEmpty(relativePath)
                ? fileName
                : relativePath + "/" + fileName;
            if (IsExcluded(relativeFilePath, false, excludedRules)) {
                continue;
            }

            var targetFile = Path.Combine(targetPath, fileName);
            File.Copy(filePath, targetFile, true);
        }

        foreach (var directoryPath in Directory.GetDirectories(sourcePath)) {
            var folderName = Path.GetFileName(directoryPath);
            if (ignoreDotPrefixedPaths && folderName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            var childRelativePath = string.IsNullOrEmpty(relativePath)
                ? folderName
                : relativePath + "/" + folderName;

            if (IsExcluded(childRelativePath, true, excludedRules)) {
                continue;
            }

            var targetChild = Path.Combine(targetPath, folderName);
            CopyDirectory(directoryPath, targetChild, childRelativePath, excludedRules, ignoreDotPrefixedPaths);
        }
    }

    private static bool IsExcluded(string relativePath, bool isFolder, IReadOnlyCollection<string> excludedRules) {
        var candidate = relativePath.Replace('\\', '/');
        return excludedRules.Any(rule => IsRuleMatch(candidate, isFolder, rule));
    }

    private static bool IsRuleMatch(string candidate, bool isFolder, string rule) {
        if (!rule.EndsWith("/", StringComparison.Ordinal)) {
            return !isFolder && candidate.Equals(rule, StringComparison.OrdinalIgnoreCase);
        }

        var folder = rule[..^1];
        return candidate.Equals(folder, StringComparison.OrdinalIgnoreCase) ||
               candidate.StartsWith(folder + "/", StringComparison.OrdinalIgnoreCase);
    }
}