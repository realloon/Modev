using System.Xml.Linq;

namespace Modev.Utility;

public static class DefBundler {
    public static void Bundle(DirectoryInfo modRoot) {
        var defsDir = new DirectoryInfo(Path.Combine(modRoot.FullName, "Defs"));
        if (!defsDir.Exists) return;

        var combined = new XElement("Defs");
        var merged = new List<FileInfo>();

        foreach (var file in defsDir.GetFiles("*.xml", SearchOption.AllDirectories)
                     .OrderBy(f => f.FullName, StringComparer.OrdinalIgnoreCase)) {
            XDocument doc;
            try {
                doc = XDocument.Load(file.FullName);
            } catch {
                continue; // leave malformed files untouched
            }

            if (doc.Root?.Name.LocalName != "Defs") continue; // not a Defs file; leave as-is

            combined.Add(doc.Root.Elements());
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
        foreach (var child in dir.GetDirectories()) {
            RemoveEmptyDirectories(child);
            if (child.GetFileSystemInfos().Length == 0) {
                child.Delete();
            }
        }
    }
}