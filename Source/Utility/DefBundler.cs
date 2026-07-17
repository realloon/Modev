using System.Xml.Linq;

namespace Modev.Utility;

public static class DefBundler {
    public static void Bundle(DirectoryInfo modRoot) {
        var defsDir = new DirectoryInfo(Path.Combine(modRoot.FullName, "Defs"));
        if (!defsDir.Exists) return;

        var combined = new XElement("Defs");
        var merged = new List<FileInfo>();

        foreach (var file in defsDir.EnumerateFiles("*.xml", SearchOption.AllDirectories)
                     .OrderBy(f => f.FullName, StringComparer.OrdinalIgnoreCase)) {
            var root = XDocument.Load(file.FullName).Root
                       ?? throw new InvalidDataException($"XML has no root element: {file.FullName}");

            if (root.Name.LocalName != "Defs") continue; // not a Defs file; leave as-is

            combined.Add(root.Elements());
            merged.Add(file);
        }

        if (merged.Count == 0) return;

        foreach (var file in merged) {
            file.Delete();
        }

        var bundlePath = Path.Combine(defsDir.FullName, "bundle.xml");
        new XDocument(new XDeclaration("1.0", "utf-8", null), combined).Save(bundlePath);

        RemoveEmptyDirectories(defsDir);
    }

    private static void RemoveEmptyDirectories(DirectoryInfo dir) {
        foreach (var child in dir.EnumerateDirectories()) {
            RemoveEmptyDirectories(child);
            if (!child.EnumerateFileSystemInfos().Any()) {
                child.Delete();
            }
        }
    }
}