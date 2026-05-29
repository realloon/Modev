namespace Modev.Utility;

public static class UploadContentFilter {
    public static DirectoryInfo BuildFilteredCopy(DirectoryInfo source, IReadOnlyCollection<string> excludedFolders,
        IReadOnlyCollection<string> excludedFiles, bool ignoreDotPrefixedPaths) {
        var root = Path.Combine(GenFilePaths.TempFolderPath, "Vortex_Modev");
        Directory.CreateDirectory(root);
        CleanupAllPreviousDirectories(root);

        var targetPath = Path.Combine(root, source.Name + "_" + DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"));
        CopyDirectory(source.FullName, targetPath, string.Empty, excludedFolders, excludedFiles,
            ignoreDotPrefixedPaths);
        return new DirectoryInfo(targetPath);
    }

    public static List<string> ListIncludedTopLevelPaths(DirectoryInfo source,
        IReadOnlyCollection<string> excludedFolders,
        IReadOnlyCollection<string> excludedFiles, bool ignoreDotPrefixedPaths) {
        var paths = new List<string>();

        foreach (var directoryPath in Directory.GetDirectories(source.FullName)
                     .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)) {
            var folderName = Path.GetFileName(directoryPath);
            if (ignoreDotPrefixedPaths && folderName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            if (IsFolderExcluded(folderName, excludedFolders)) {
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

            if (IsFileExcluded(fileName, excludedFiles)) {
                continue;
            }

            paths.Add(fileName);
        }

        return paths;
    }

    private static void CleanupAllPreviousDirectories(string rootPath) {
        DirectoryInfo[] dirs;
        try {
            dirs = new DirectoryInfo(rootPath).GetDirectories();
        } catch (Exception ex) {
            Log.Warning($"Modev: Failed to enumerate temp upload directories: {ex.Message}");
            return;
        }

        foreach (var dir in dirs) {
            TryDeleteDirectory(dir);
        }
    }

    private static void TryDeleteDirectory(DirectoryInfo dir) {
        try {
            dir.Delete(true);
        } catch (Exception ex) {
            Log.Warning($"Modev: Failed to delete temp upload directory '{dir.FullName}': {ex.Message}");
        }
    }

    private static void CopyDirectory(string sourcePath, string targetPath, string relativePath,
        IReadOnlyCollection<string> excludedFolders, IReadOnlyCollection<string> excludedFiles,
        bool ignoreDotPrefixedPaths) {
        Directory.CreateDirectory(targetPath);

        foreach (var filePath in Directory.GetFiles(sourcePath)) {
            var fileName = Path.GetFileName(filePath);
            if (ignoreDotPrefixedPaths && fileName.StartsWith(".", StringComparison.Ordinal)) {
                continue;
            }

            var relativeFilePath = string.IsNullOrEmpty(relativePath)
                ? fileName
                : relativePath + "/" + fileName;
            if (IsFileExcluded(relativeFilePath, excludedFiles)) {
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

            if (IsFolderExcluded(childRelativePath, excludedFolders)) {
                continue;
            }

            var targetChild = Path.Combine(targetPath, folderName);
            CopyDirectory(directoryPath, targetChild, childRelativePath, excludedFolders, excludedFiles,
                ignoreDotPrefixedPaths);
        }
    }

    private static bool IsFolderExcluded(string relativeFolderPath, IReadOnlyCollection<string> excludedFolders) {
        var candidate = relativeFolderPath.Replace('\\', '/');
        return excludedFolders.Any(excludedFolder =>
            candidate.Equals(excludedFolder, StringComparison.OrdinalIgnoreCase) ||
            candidate.StartsWith(excludedFolder + "/", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsFileExcluded(string relativeFilePath, IReadOnlyCollection<string> excludedFiles) {
        var candidate = relativeFilePath.Replace('\\', '/');
        return excludedFiles.Any(excludedFile => candidate.Equals(excludedFile, StringComparison.OrdinalIgnoreCase));
    }
}
