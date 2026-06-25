#if DEBUG

namespace Modev.Utility;

public static class DebugExport {
    public static void ExportToDesktop(ModMetaData mod) {
        var source = mod.GetWorkshopUploadDirectory();
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var target = Path.Combine(desktop, mod.RootDir.Name + "_" + DateTime.UtcNow.ToString("MMddHHmmssfff"));

        CopyDirectory(source.FullName, target);
    }

    private static void CopyDirectory(string sourcePath, string targetPath) {
        Directory.CreateDirectory(targetPath);

        foreach (var filePath in Directory.GetFiles(sourcePath)) {
            File.Copy(filePath, Path.Combine(targetPath, Path.GetFileName(filePath)), true);
        }

        foreach (var directoryPath in Directory.GetDirectories(sourcePath)) {
            CopyDirectory(directoryPath, Path.Combine(targetPath, Path.GetFileName(directoryPath)));
        }
    }
}
#endif