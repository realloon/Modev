#if DEBUG
namespace Modev.Utility;

public static class DebugExport {
    public static void ExportToDesktop(ModMetaData mod) {
        var source = mod.GetWorkshopUploadDirectory();
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        var target = Path.Combine(desktop, mod.RootDir.Name + "_" + DateTime.UtcNow.ToString("MMddHHmmssfff"));

        UploadContentFilter.CopyDirectory(source.FullName, target);
    }
}
#endif